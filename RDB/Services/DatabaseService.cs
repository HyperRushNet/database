using RDB.Models;
using System.Text.Json;

namespace RDB.Services;

public class DatabaseService : IStorageService
{
    private readonly string _dataDir = "data";
    private readonly Dictionary<string, List<ItemEnvelope>> _index = new();

    public DatabaseService()
    {
        if (!Directory.Exists(_dataDir)) Directory.CreateDirectory(_dataDir);
    }

    public ItemEnvelope AddItem(string type, object payload)
    {
        var id = Guid.NewGuid().ToString("N");
        var relativePath = $"{type}/{id.Substring(0,2)}/{id.Substring(2,2)}/{id}.json";
        var fullPath = Path.Combine(_dataDir, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        var envelope = new ItemEnvelope
        {
            Id = id,
            Type = type,
            CreatedAt = DateTime.UtcNow,
            Payload = payload ?? new {},
            RelativePath = relativePath
        };

        var json = JsonSerializer.Serialize(envelope, new JsonSerializerOptions { WriteIndented = false });
        File.WriteAllText(fullPath, json);
        envelope.SizeBytes = new FileInfo(fullPath).Length;

        if (!_index.ContainsKey(type)) _index[type] = new();
        _index[type].Add(envelope);

        return envelope;
    }

    public ItemEnvelope? GetItem(string type, string id)
    {
        if (!_index.ContainsKey(type)) return null;
        return _index[type].FirstOrDefault(i => i.Id == id);
    }

    public IEnumerable<ItemEnvelope> GetItems(string type)
    {
        if (!_index.ContainsKey(type)) return Enumerable.Empty<ItemEnvelope>();
        return _index[type];
    }

    public bool DeleteItem(string type, string id)
    {
        var item = GetItem(type, id);
        if (item == null) return false;

        var fullPath = Path.Combine(_dataDir, item.RelativePath.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath)) File.Delete(fullPath);
        _index[type].Remove(item);
        return true;
    }
}
