using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RDB.Services
{
    public class DatabaseService
    {
        private readonly string baseDir = "data";
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ItemEnvelope>> index
            = new();
        private readonly SemaphoreSlim writeLock = new(1,1);
        private readonly string indexFile = "data/index.json";

        public DatabaseService()
        {
            Directory.CreateDirectory(baseDir);
            LoadIndex();
            StartPeriodicSave();
        }

        // Add new item
        public ItemEnvelope AddItem(string type, object payload)
        {
            var item = new ItemEnvelope
            {
                Id = Guid.NewGuid().ToString("N"),
                Type = type,
                CreatedAt = DateTime.UtcNow,
                Payload = payload
            };
            var path = GetFilePath(type, item.Id);
            item.RelativePath = path;
            var bytes = JsonSerializer.SerializeToUtf8Bytes(item);
            item.SizeBytes = bytes.Length;

            writeLock.Wait();
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllBytes(path, bytes);
                if (!index.ContainsKey(type)) index[type] = new ConcurrentDictionary<string, ItemEnvelope>();
                index[type][item.Id] = item;
            }
            finally { writeLock.Release(); }

            return item;
        }

        // Get single item
        public ItemEnvelope? GetItem(string type, string id)
        {
            if(index.ContainsKey(type) && index[type].ContainsKey(id))
                return index[type][id];
            return null;
        }

        // List items with optional filtering and pagination
        public List<ItemEnvelope> GetItems(string type, int skip=0, int limit=50, Dictionary<string,string>? filters=null)
        {
            if(!index.ContainsKey(type)) return new();
            var items = index[type].Values.AsEnumerable();
            if(filters != null)
            {
                foreach(var f in filters)
                {
                    items = items.Where(i => {
                        try {
                            var json = JsonSerializer.Serialize(i.Payload);
                            using var doc = JsonDocument.Parse(json);
                            return doc.RootElement.TryGetProperty(f.Key, out var v) && v.GetRawText().Trim('"') == f.Value;
                        } catch { return false; }
                    });
                }
            }
            return items.Skip(skip).Take(limit).ToList();
        }

        // Delete item
        public bool DeleteItem(string type, string id)
        {
            writeLock.Wait();
            try
            {
                if(index.ContainsKey(type) && index[type].TryRemove(id, out var item))
                {
                    if(File.Exists(item.RelativePath)) File.Delete(item.RelativePath);
                    return true;
                }
                return false;
            }
            finally { writeLock.Release(); }
        }

        // Edit / update item
        public ItemEnvelope? UpdateItem(string type, string id, object newPayload)
        {
            writeLock.Wait();
            try
            {
                if(index.ContainsKey(type) && index[type].ContainsKey(id))
                {
                    var item = index[type][id];
                    item.Payload = newPayload;
                    var bytes = JsonSerializer.SerializeToUtf8Bytes(item);
                    item.SizeBytes = bytes.Length;
                    File.WriteAllBytes(item.RelativePath, bytes);
                    index[type][id] = item;
                    return item;
                }
                return null;
            }
            finally { writeLock.Release(); }
        }

        // Return all types
        public List<string> GetAllTypes() => index.Keys.ToList();

        // Load index from disk
        private void LoadIndex()
        {
            if(File.Exists(indexFile))
            {
                try
                {
                    var json = File.ReadAllText(indexFile);
                    var dict = JsonSerializer.Deserialize<Dictionary<string,List<ItemEnvelope>>>(json);
                    if(dict != null)
                    {
                        foreach(var kv in dict)
                        {
                            index[kv.Key] = new ConcurrentDictionary<string, ItemEnvelope>(
                                kv.Value.ToDictionary(i=>i.Id, i=>i)
                            );
                        }
                    }
                }
                catch { /* ignore corrupted index */ }
            }
            else
            {
                // Build index from files if index.json does not exist
                foreach(var dir in Directory.GetDirectories(baseDir))
                {
                    var type = Path.GetFileName(dir);
                    index[type] = new ConcurrentDictionary<string, ItemEnvelope>();
                    foreach(var file in Directory.GetFiles(dir, "*.json", SearchOption.AllDirectories))
                    {
                        try
                        {
                            var bytes = File.ReadAllBytes(file);
                            var item = JsonSerializer.Deserialize<ItemEnvelope>(bytes);
                            if(item!=null) index[type][item.Id]=item;
                        } catch { }
                    }
                }
            }
        }

        // Periodically save index every 30 seconds
        private void StartPeriodicSave()
        {
            Task.Run(async () =>
            {
                while(true)
                {
                    await Task.Delay(TimeSpan.FromSeconds(30));
                    SaveIndex();
                }
            });
        }

        private void SaveIndex()
        {
            try
            {
                var dict = index.ToDictionary(
                    kv => kv.Key,
                    kv => kv.Value.Values.ToList()
                );
                var json = JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = false });
                File.WriteAllText(indexFile, json);
            }
            catch { /* ignore save errors */ }
        }
    }

    public class ItemEnvelope
    {
        public string Id {get;set;} = "";
        public string Type {get;set;} = "";
        public DateTime CreatedAt {get;set;}
        public object Payload {get;set;} = new {};
        public string RelativePath {get;set;} = "";
        public long SizeBytes {get;set;}
    }
}
