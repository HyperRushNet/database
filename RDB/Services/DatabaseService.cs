using System.Text.Json;
using System.Collections.Concurrent;

namespace RDB.Services;

public class ItemEnvelope
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Type { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Payload { get; set; } = new();
}

public class DatabaseService
{
    private readonly string _root = "data";
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ItemEnvelope>> _cache
        = new();

    public DatabaseService()
    {
        Directory.CreateDirectory(_root);
    }

    public Task<ItemEnvelope> AddItem(string type, Dictionary<string, object> payload)
    {
        var item = new ItemEnvelope { Type = type, Payload = payload };
        var dict = _cache.GetOrAdd(type, _ => new ConcurrentDictionary<string, ItemEnvelope>());
        dict[item.Id] = item;

        SaveItem(type, item);
        return Task.FromResult(item);
    }

    public Task<List<ItemEnvelope>> GetItems(string type)
    {
        if (_cache.TryGetValue(type, out var dict))
            return Task.FromResult(dict.Values.ToList());
        return Task.FromResult(new List<ItemEnvelope>());
    }

    public Task<ItemEnvelope?> GetItem(string type, string id)
    {
        if (_cache.TryGetValue(type, out var dict) && dict.TryGetValue(id, out var item))
            return Task.FromResult<ItemEnvelope?>(item);
        return Task.FromResult<ItemEnvelope?>(null);
    }

    public Task<bool> DeleteItem(string type, string id)
    {
        if (_cache.TryGetValue(type, out var dict) && dict.TryRemove(id, out var _))
        {
            var path = Path.Combine(_root, type, $"{id}.json");
            if (File.Exists(path)) File.Delete(path);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    private void SaveItem(string type, ItemEnvelope item)
    {
        var dir = Path.Combine(_root, type);
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, $"{item.Id}.json");
        File.WriteAllText(path, JsonSerializer.Serialize(item));
    }
}
