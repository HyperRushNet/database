using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using RDB.Models;

namespace RDB.Services
{
    public class DatabaseService
    {
        private readonly string _dataDir = "/data";
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ItemEnvelope>> _index 
            = new ConcurrentDictionary<string, ConcurrentDictionary<string, ItemEnvelope>>();

        public DatabaseService()
        {
            if(!Directory.Exists(_dataDir)) Directory.CreateDirectory(_dataDir);
        }

        private string GetFilePath(string type, string id)
        {
            var parts = new[] { id.Substring(0, 2), id.Substring(2, 2), id };
            var path = Path.Combine(_dataDir, type, Path.Combine(parts)) + ".json";
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            return path;
        }

        public ItemEnvelope AddItem(string type, object payload)
        {
            var id = Guid.NewGuid().ToString("N");
            var createdAt = DateTime.UtcNow;
            var path = GetFilePath(type, id);
            var jsonItem = new ItemEnvelope
            {
                Id = id,
                Type = type,
                CreatedAt = createdAt,
                Payload = payload,
                RelativePath = path.Replace(_dataDir+"/", ""),
                SizeBytes = 0
            };
            File.WriteAllText(path, JsonSerializer.Serialize(jsonItem));
            var typeDict = _index.GetOrAdd(type, new ConcurrentDictionary<string, ItemEnvelope>());
            typeDict[id] = jsonItem;
            return jsonItem;
        }

        public ItemEnvelope? GetItem(string type, string id)
        {
            if(_index.TryGetValue(type, out var typeDict))
            {
                if(typeDict.TryGetValue(id, out var item)) return item;
            }
            var path = GetFilePath(type, id);
            if(File.Exists(path))
            {
                var item = JsonSerializer.Deserialize<ItemEnvelope>(File.ReadAllText(path));
                if(item != null)
                {
                    var typeDict = _index.GetOrAdd(type, new ConcurrentDictionary<string, ItemEnvelope>());
                    typeDict[id] = item;
                }
                return item;
            }
            return null;
        }

        public List<ItemEnvelope> GetItems(string type, int skip=0, int limit=50, Dictionary<string,string>? filters=null)
        {
            if(!_index.TryGetValue(type, out var typeDict)) return new List<ItemEnvelope>();
            var items = new List<ItemEnvelope>(typeDict.Values);
            if(filters!=null)
            {
                items = items.FindAll(i =>
                {
                    foreach(var kv in filters)
                    {
                        if(!i.PayloadAsDict().ContainsKey(kv.Key) || i.PayloadAsDict()[kv.Key]?.ToString() != kv.Value) return false;
                    }
                    return true;
                });
            }
            return items.GetRange(Math.Min(skip, items.Count), Math.Min(limit, items.Count - skip));
        }

        public bool DeleteItem(string type, string id)
        {
            var path = GetFilePath(type, id);
            if(File.Exists(path)) File.Delete(path);
            if(_index.TryGetValue(type, out var typeDict)) return typeDict.TryRemove(id, out _);
            return false;
        }

        public List<string> GetAllTypes()
        {
            return new List<string>(Directory.GetDirectories(_dataDir).Select(d => Path.GetFileName(d)));
        }
    }
}
