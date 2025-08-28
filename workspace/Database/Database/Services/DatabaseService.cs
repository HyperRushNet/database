using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Database.Models;

namespace Database.Services
{
    public class DatabaseService : IStorageService, IDisposable
    {
        private readonly string _baseDir;
        private readonly string _indexPath;
        private readonly SemaphoreSlim _indexLock = new SemaphoreSlim(1,1);
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        private readonly ConcurrentQueue<ItemEnvelope> _writeQueue = new ConcurrentQueue<ItemEnvelope>();
        private readonly int _batchFlushThreshold;
        private readonly TimeSpan _batchFlushInterval;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private Task? _backgroundFlushTask;
        private ConcurrentDictionary<string, List<IndexEntry>>? _index = null;

        public DatabaseService(string baseDir, int batchFlushThreshold = 10, TimeSpan? batchFlushInterval = null)
        {
            _baseDir = string.IsNullOrWhiteSpace(baseDir) ? "data" : baseDir;
            _indexPath = Path.Combine(_baseDir, "index.json");
            Directory.CreateDirectory(_baseDir);

            _batchFlushThreshold = Math.Max(1, batchFlushThreshold);
            _batchFlushInterval = batchFlushInterval ?? TimeSpan.FromSeconds(5);

            _backgroundFlushTask = Task.Run(BackgroundFlushLoop);
        }

        private void EnsureIndexLoaded()
        {
            if (_index != null) return;

            _indexLock.Wait();
            try
            {
                if (_index != null) return;
                var dict = new ConcurrentDictionary<string, List<IndexEntry>>(StringComparer.OrdinalIgnoreCase);
                if (File.Exists(_indexPath))
                {
                    try
                    {
                        using var fs = File.OpenRead(_indexPath);
                        var map = JsonSerializer.Deserialize<Dictionary<string, List<IndexEntry>>>(fs) ?? new Dictionary<string, List<IndexEntry>>();
                        foreach (var kv in map)
                            dict[kv.Key] = kv.Value;
                    }
                    catch
                    {
                        var badPath = _indexPath + ".bad-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                        try { File.Move(_indexPath, badPath); } catch { }
                    }
                }
                _index = dict;
            }
            finally
            {
                _indexLock.Release();
            }
        }

        private string MakeFilePath(string type, string id)
        {
            var safeType = SanitizePathComponent(type);
            var sub1 = id.Length >= 2 ? id.Substring(0,2) : id;
            var sub2 = id.Length >= 4 ? id.Substring(2,2) : (id.Length > 2 ? id.Substring(2) : "");
            var folder = Path.Combine(_baseDir, safeType, sub1, sub2);
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, id + ".json");
        }

        private static string SanitizePathComponent(string s)
        {
            foreach (var c in Path.GetInvalidPathChars()) s = s.Replace(c, '_');
            s = s.Replace("..", "_");
            return s;
        }

        public Task<IndexEntry> StoreItemAsync(string type, JsonElement payload)
        {
            var env = new ItemEnvelope
            {
                Id = Guid.NewGuid().ToString("n"),
                Type = type,
                Payload = payload,
                CreatedAt = DateTime.UtcNow
            };

            var filePath = MakeFilePath(type, env.Id);
            env.Path = Path.GetRelativePath(_baseDir, filePath).Replace('\\', '/');

            _writeQueue.Enqueue(env);

            if (_writeQueue.Count >= _batchFlushThreshold)
            {
                _ = Task.Run(() => FlushQueueAsync());
            }

            var indexEntry = new IndexEntry
            {
                Id = env.Id,
                Type = env.Type,
                RelativePath = env.Path,
                CreatedAt = env.CreatedAt,
                SizeBytes = 0
            };

            EnsureIndexLoaded();
            _index!.AddOrUpdate(type,
                (_) => new List<IndexEntry> { indexEntry },
                (_, list) => { lock (list) { list.Add(indexEntry); } return list; });

            return Task.FromResult(indexEntry);
        }

        public async Task<IEnumerable<IndexEntry>> StoreBatchAsync(string type, IEnumerable<JsonElement> payloads)
        {
            var results = new List<IndexEntry>();
            foreach (var p in payloads)
            {
                var r = await StoreItemAsync(type, p);
                results.Add(r);
            }
            _ = Task.Run(() => FlushQueueAsync());
            return results;
        }

        public async Task<ItemEnvelope?> GetItemAsync(string type, string id)
        {
            var filePath = MakeFilePath(type, id);
            if (!File.Exists(filePath)) return null;

            using var fs = File.OpenRead(filePath);
            return await JsonSerializer.DeserializeAsync<ItemEnvelope>(fs);
        }

        public Task<IEnumerable<IndexEntry>> ListTypeAsync(string type, int skip = 0, int take = 100)
        {
            EnsureIndexLoaded();
            if (!_index!.TryGetValue(type, out var list))
                return Task.FromResult(Enumerable.Empty<IndexEntry>());

            lock (list)
            {
                var page = list.OrderByDescending(e => e.CreatedAt).Skip(skip).Take(take).ToList();
                return Task.FromResult<IEnumerable<IndexEntry>>(page);
            }
        }

        private async Task FlushQueueAsync()
        {
            if (!_indexLock.Wait(0)) return;
            try
            {
                var toFlush = new List<ItemEnvelope>();
                while (_writeQueue.TryDequeue(out var item))
                {
                    toFlush.Add(item);
                    if (toFlush.Count >= 1000) break;
                }

                if (toFlush.Count == 0) return;

                foreach (var env in toFlush)
                {
                    var absPath = Path.Combine(_baseDir, env.Path);
                    var tempPath = absPath + ".tmp";
                    try
                    {
                        using (var fs = File.Create(tempPath))
                        {
                            await JsonSerializer.SerializeAsync(fs, env, _jsonOptions);
                            await fs.FlushAsync();
                        }
                        if (File.Exists(absPath)) File.Delete(absPath);
                        File.Move(tempPath, absPath);

                        var fi = new FileInfo(absPath);
                        EnsureIndexLoaded();
                        if (_index!.TryGetValue(env.Type, out var lst))
                        {
                            lock (lst)
                            {
                                var entry = lst.FirstOrDefault(e => e.Id == env.Id);
                                if (entry != null) entry.SizeBytes = fi.Length;
                            }
                        }
                    }
                    catch
                    {
                        try { if (File.Exists(tempPath)) File.Delete(tempPath); } catch { }
                    }
                }

                try
                {
                    var map = _index!.ToDictionary(kv => kv.Key, kv => kv.Value);
                    var tempIndex = _indexPath + ".tmp";
                    using (var fs = File.Create(tempIndex))
                    {
                        await JsonSerializer.SerializeAsync(fs, map, _jsonOptions);
                        await fs.FlushAsync();
                    }
                    if (File.Exists(_indexPath)) File.Delete(_indexPath);
                    File.Move(tempIndex, _indexPath);
                }
                catch { }
            }
            finally
            {
                _indexLock.Release();
            }
        }

        private async Task BackgroundFlushLoop()
        {
            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    await Task.Delay(_batchFlushInterval, _cts.Token);
                    try { await FlushQueueAsync(); } catch { }
                }
            }
            catch (TaskCanceledException) { }
        }

        public void Dispose()
        {
            _cts.Cancel();
            try { _backgroundFlushTask?.Wait(2000); } catch { }
            _cts.Dispose();
            _indexLock.Dispose();
        }
    }
}
