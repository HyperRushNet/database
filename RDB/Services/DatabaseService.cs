
using RDB.Models;
using System.Text.Json;

namespace RDB.Services;

public class DatabaseService
{
    private readonly string _rootFolder;
    public DatabaseService(string rootFolder)
    {
        _rootFolder = rootFolder;
        Directory.CreateDirectory(_rootFolder);
    }

    private string GetFilePath(string type, string id) =>
        Path.Combine(_rootFolder, type, $"{id}.json");

    public Task<ItemEnvelope> AddItem(string type, Dictionary<string, object> payload)
    {
        var id = Guid.NewGuid().ToString("N");
        var folder = Path.Combine(_rootFolder, type);
        Directory.CreateDirectory(folder);

        var item = new ItemEnvelope
        {
            Id = id,
            Type = type,
            RelativePath = Path.Combine(type, $"{id}.json"),
            CreatedAt = DateTime.UtcNow,
            Content = payload
        };

        File.WriteAllText(GetFilePath(type, id), JsonSerializer.Serialize(item.Content));
        return Task.FromResult(item);
    }

    public Task<ItemEnvelope?> GetItem(string type, string id)
    {
        var path = GetFilePath(type, id);
        if (!File.Exists(path)) return Task.FromResult<ItemEnvelope?>(null);

        var content = JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(path)) ?? new();
        var item = new ItemEnvelope
        {
            Id = id,
            Type = type,
            RelativePath = Path.Combine(type, $"{id}.json"),
            CreatedAt = File.GetCreationTimeUtc(path),
            Content = content
        };
        return Task.FromResult<ItemEnvelope?>(item);
    }

    public Task<bool> DeleteItem(string type, string id)
    {
        var path = GetFilePath(type, id);
        if (!File.Exists(path)) return Task.FromResult(false);
        File.Delete(path);
        return Task.FromResult(true);
    }

    public Task<List<ItemEnvelope>> GetAllItems(string type)
    {
        var folder = Path.Combine(_rootFolder, type);
        if (!Directory.Exists(folder)) return Task.FromResult(new List<ItemEnvelope>());

        var items = new List<ItemEnvelope>();
        foreach (var file in Directory.EnumerateFiles(folder, "*.json"))
        {
            var id = Path.GetFileNameWithoutExtension(file);
            var content = JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(file)) ?? new();
            items.Add(new ItemEnvelope
            {
                Id = id,
                Type = type,
                RelativePath = Path.Combine(type, $"{id}.json"),
                CreatedAt = File.GetCreationTimeUtc(file),
                Content = content
            });
        }
        return Task.FromResult(items);
    }
}
