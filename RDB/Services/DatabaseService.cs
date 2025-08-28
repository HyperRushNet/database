using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var folder = Path.Combine(_dataDir, type, Path.Combine(parts.Take(2).ToArray()));
            if(!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            return Path.Combine(folder, id + ".json");
        }

        public ItemEnvelope AddItem(string type, object payload)
        {
            var id = Guid.NewGuid().ToString("N");
            var createdAt = DateTime.UtcNow;
            var path = GetFilePath(type, id);
            var item = new ItemEnvelope
            {
                Id = id,
                Type = type,
                CreatedAt = createdAt,
                Payload = payload,
                RelativePath = path.Replace(_dataDir+"/", ""),
                SizeBytes = 0
            };
            File.WriteAllText(path, JsonSerializer.Serialize(item));
            var dict = _index.GetOrAdd(type, new ConcurrentDictionary<string, ItemEnvelope>());
            dict[id] = item;
            return item;
        }

        public ItemEnvelope? GetItem(string type, string id)
        {
            if(_index.TryGetValue(type, out var dict))
            {
                if(dict.TryGetValue(id, out var item)) return item;
            }
            var path = GetFilePath(type, id);
            if(File.Exists(path))
            {
                var item = JsonSerializer.Deserialize<ItemEnvelope>(File.ReadAllText(path));
                if(item != null)
                {
                    var dict2 = _index.GetOrAdd(type, new ConcurrentDictionary<string, ItemEnvelope>());
                    dict2[id] = item;
                }
                return item;
            }
            return null;
        }

        public List<ItemEnvelope> GetItems(string type, int skip=0, int limit=50, Dictionary<string,string>? filters=null)
        {
            if(!_index.TryGetValue(type, out var dict)) return new List<ItemEnvelope>();
            var items = dict.Values.ToList();
            if(filters != null && filters.Count > 0)
            {
                items = items.Where(i =>
                {
                    var pd = i.PayloadAsDict();
                    return filters.All(f => pd.ContainsKey(f.Key) && pd[f.Key]?.ToString() == f.Value);
                }).ToList();
            }
            skip = Math.Min(skip, items.Count);
            limit = Math.Min(limit, items.Count - skip);
            return items.GetRange(skip, limit);
        }

        public bool DeleteItem(string type, string id)
        {
            var path = GetFilePath(type, id);
            if(File.Exists(path)) File.Delete(path);
            if(_index.TryGetValue(type, out var dict))
            {
                return dict.TryRemove(id, out _);
            }
            return false;
        }

        public List<string> GetAllTypes()
        {
            if(!Directory.Exists(_dataDir)) return new List<string>();
            return Directory.GetDirectories(_dataDir).Select(d => Path.GetFileName(d)).ToList();
        }
    }
}
