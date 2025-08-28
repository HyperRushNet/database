using System.Text.Json;
using RDB.Models;
using System.Collections.Concurrent;

namespace RDB.Services
{
    public class DatabaseService
    {
        private readonly ConcurrentDictionary<string, ItemEnvelope> _index = new();
        private readonly ConcurrentQueue<ItemEnvelope> _writeQueue = new();
        private readonly string _dataDir = "data";

        public DatabaseService()
        {
            LoadAllItems();
            StartBatchFlush();
        }

        private void LoadAllItems()
        {
            if (!Directory.Exists(_dataDir)) return;
            foreach (var typeDir in Directory.GetDirectories(_dataDir))
            {
                foreach (var file in Directory.GetFiles(typeDir, "*.json", SearchOption.AllDirectories))
                {
                    var json = File.ReadAllText(file);
                    var item = JsonSerializer.Deserialize<ItemEnvelope>(json);
                    if (item != null)
                        _index[$"{item.Type}:{item.Id}"] = item;
                }
            }
        }

        private void StartBatchFlush()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    if (_writeQueue.Count > 0)
                    {
                        List<ItemEnvelope> batch = new();
                        while (_writeQueue.TryDequeue(out var item)) batch.Add(item);

                        foreach (var item in batch)
                        {
                            var path = Path.Combine(_dataDir, item.RelativePath);
                            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                            File.WriteAllText(path, JsonSerializer.Serialize(item));
                        }
                    }
                    await Task.Delay(1000); // flush every second
                }
            });
        }

        public ItemEnvelope AddItem(string type, object payload)
        {
            var id = Guid.NewGuid().ToString("N");
            var item = new ItemEnvelope
            {
                Id = id,
                Type = type,
                CreatedAt = DateTime.UtcNow,
                Payload = payload,
                RelativePath = $"{type}/{id.Substring(0,2)}/{id.Substring(2,2)}/{id}.json"
            };
            _index[$"{type}:{item.Id}"] = item;
            _writeQueue.Enqueue(item);
            return item;
        }

        public IEnumerable<ItemEnvelope> GetItems(string type)
            => _index.Values.Where(i => i.Type == type);

        public ItemEnvelope? GetItem(string type, string id)
            => _index.TryGetValue($"{type}:{id}", out var item) ? item : null;

        public bool DeleteItem(string type, string id)
        {
            if (!_index.TryGetValue($"{type}:{id}", out var item)) return false;
            _index.TryRemove($"{type}:{id}", out _);
            var path = Path.Combine(_dataDir, item.RelativePath);
            if (File.Exists(path)) File.Delete(path);
            return true;
        }
    }
}
