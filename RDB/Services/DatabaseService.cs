using System.Text.Json;
using RDB.Models;

namespace RDB.Services;

public class DatabaseService
{
    private readonly string _root = "data";
    private readonly Dictionary<string, Dictionary<string, ItemEnvelope>> _cache = new();

    public DatabaseService()
    {
        if (!Directory.Exists(_root)) Directory.CreateDirectory(_root);
    }

    private string GetFilePath(string type, string id)
    {
        var p1 = id.Substring(0, 2);
        var p2 = id.Substring(2, 2);
        var dir = Path.Combine(_root, type, p1, p2);
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, id + ".json");
    }

    public async Task<ItemEnvelope> AddItem(string type, Dictionary<string, object> payload)
    {
        var id = Guid.NewGuid().ToString("N");
        var item = new ItemEnvelope
        {
            Id = id,
            Type = type,
            CreatedAt = DateTime.UtcNow,
            Payload = payload,
            RelativePath = GetFilePath(type, id),
            SizeBytes = 0
        };
        var json = JsonSerializer.Serialize(item);
        await File.WriteAllTextAsync(item.RelativePath, json);
        item.SizeBytes = json.Length;

        if (!_cache.ContainsKey(type)) _cache[type] = new();
        _cache[type][id] = item;

        return new ItemEnvelope { Id = id };
    }

    public async Task<ItemEnvelope?> GetItem(string type, string id)
    {
        var path = GetFilePath(type, id);
        if (!File.Exists(path)) return null;
        var json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<ItemEnvelope>(json);
    }

    public async Task<List<ItemEnvelope>> ListItems(string type)
    {
        var items = new List<ItemEnvelope>();
        var typeDir = Path.Combine(_root, type);
        if (!Directory.Exists(typeDir)) return items;

        foreach (var file in Directory.EnumerateFiles(typeDir, "*.json", SearchOption.AllDirectories))
        {
            var json = await File.ReadAllTextAsync(file);
            var item = JsonSerializer.Deserialize<ItemEnvelope>(json);
            if (item != null) items.Add(item);
        }
        return items;
    }

    public async Task<bool> DeleteItem(string type, string id)
    {
        var path = GetFilePath(type, id);
        if (File.Exists(path)) File.Delete(path);
        _cache.GetValueOrDefault(type)?.Remove(id);
        return true;
    }

    public List<string> ListTypes() => Directory.Exists(_root) ? Directory.GetDirectories(_root).Select(Path.GetFileName).ToList() : new();
}
