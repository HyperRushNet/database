using RDB.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace RDB.Services
{
    public class DatabaseService
    {
        private readonly string _basePath = "data";

        public DatabaseService()
        {
            Directory.CreateDirectory(_basePath);
        }

        public async Task<ItemEnvelope> AddItem(string type, Dictionary<string, object> payload)
        {
            var id = Guid.NewGuid().ToString("N");
            var item = new ItemEnvelope
            {
                Id = id,
                Type = type,
                CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Payload = payload,
                RelativePath = $"{type}/{id}.json",
                SizeBytes = 0
            };

            var path = Path.Combine(_basePath, type);
            Directory.CreateDirectory(path);
            var fullPath = Path.Combine(path, $"{id}.json");
            var json = JsonSerializer.Serialize(item);
            await File.WriteAllTextAsync(fullPath, json);
            item.SizeBytes = new FileInfo(fullPath).Length;

            return item;
        }

        public async Task<ItemEnvelope> GetItem(string type, string id)
        {
            var fullPath = Path.Combine(_basePath, type, $"{id}.json");
            if (!File.Exists(fullPath)) return null;
            var json = await File.ReadAllTextAsync(fullPath);
            return JsonSerializer.Deserialize<ItemEnvelope>(json);
        }

        public async Task<List<ItemEnvelope>> GetItems(string type)
        {
            var path = Path.Combine(_basePath, type);
            var list = new List<ItemEnvelope>();
            if (!Directory.Exists(path)) return list;
            foreach (var file in Directory.GetFiles(path, "*.json"))
            {
                var json = await File.ReadAllTextAsync(file);
                var item = JsonSerializer.Deserialize<ItemEnvelope>(json);
                if (item != null) list.Add(item);
            }
            return list;
        }

        public async Task<bool> DeleteItem(string type, string id)
        {
            var fullPath = Path.Combine(_basePath, type, $"{id}.json");
            if (!File.Exists(fullPath)) return false;
            await Task.Run(() => File.Delete(fullPath));
            return true;
        }
    }
}
