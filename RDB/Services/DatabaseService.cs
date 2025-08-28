using System.Text.Json;
using RDB.Models;

namespace RDB.Services;

public class DatabaseService : IStorageService
{
    private readonly string _root = Path.Combine(AppContext.BaseDirectory, "data");

    public DatabaseService()
    {
        Directory.CreateDirectory(_root);
    }

    public async Task SaveItemAsync(ItemEnvelope item)
    {
        string typeDir = Path.Combine(_root, item.Type);
        Directory.CreateDirectory(typeDir);
        string filePath = Path.Combine(typeDir, item.Id + ".json");
        var json = JsonSerializer.Serialize(item);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<ItemEnvelope?> GetItemAsync(string type, string id)
    {
        string filePath = Path.Combine(_root, type, id + ".json");
        if (!File.Exists(filePath)) return null;
        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<ItemEnvelope>(json);
    }

    public async Task<List<ItemEnvelope>> GetAllItemsAsync(string type)
    {
        string typeDir = Path.Combine(_root, type);
        if (!Directory.Exists(typeDir)) return new List<ItemEnvelope>();
        var files = Directory.GetFiles(typeDir, "*.json", SearchOption.TopDirectoryOnly);
        var list = new List<ItemEnvelope>();
        foreach(var f in files)
        {
            var json = await File.ReadAllTextAsync(f);
            var item = JsonSerializer.Deserialize<ItemEnvelope>(json);
            if(item != null) list.Add(item);
        }
        return list;
    }

    public Task<bool> DeleteItemAsync(string type, string id)
    {
        string filePath = Path.Combine(_root, type, id + ".json");
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}
