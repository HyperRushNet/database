using System.Text.Json;
using System.Collections.Concurrent;
using RDB.Models;

namespace RDB.Services
{
    public class DatabaseService
    {
        private readonly ConcurrentDictionary<string, ItemEnvelope> _index = new();
        private readonly ConcurrentQueue<ItemEnvelope> _writeQueue = new();
        private readonly string _dataDir = "data";

        public DatabaseService()
        {
            if(!Directory.Exists(_dataDir)) Directory.CreateDirectory(_dataDir);
            foreach(var typeDir in Directory.GetDirectories(_dataDir))
            {
                foreach(var file in Directory.GetFiles(typeDir, "*.json", SearchOption.AllDirectories))
                {
                    var json = File.ReadAllText(file);
                    var item = JsonSerializer.Deserialize<ItemEnvelope>(json);
                    if(item != null) _index[$"{item.Type}:{item.Id}"] = item;
                }
            }
            Task.Run(async () =>
            {
                while(true)
                {
                    if(_writeQueue.Count > 0)
                    {
                        var batch = new List<ItemEnvelope>();
                        while(_writeQueue.TryDequeue(out var item)) batch.Add(item);
                        foreach(var item in batch)
                        {
                            var path = Path.Combine(_dataDir, item.RelativePath);
                            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                            File.WriteAllText(path, JsonSerializer.Serialize(item));
                        }
                    }
                    await Task.Delay(1000);
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

        public IEnumerable
