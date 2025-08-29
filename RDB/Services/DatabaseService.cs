using System.IO.Compression;
using Microsoft.Extensions.Caching.Memory;
using RDB.Models;

namespace RDB.Services;

public class DatabaseService : IStorageService
{
    private readonly string _root = Path.Combine(AppContext.BaseDirectory, "data");
    private readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

    public DatabaseService()
    {
        Directory.CreateDirectory(_root);
    }

    private string GetFilePath(string type, string id) =>
        Path.Combine(_root, type, id.Substring(0, 2), id.Substring(2, 2), id + ".json.gz");

    public async Task SaveItemAsync(ItemEnvelope item)
    {
        string typeDir = Path.Combine(_root, item.Type, item.Id.Substring(0, 2), item.Id.Substring(2, 2));
        Directory.CreateDirectory(typeDir);
        string filePath = GetFilePath(item.Type, item.Id);

        await using var fs = File.Create(filePath);
        await using var gzip = new GZipStream(fs, CompressionLevel.Fastest);
        await JsonSerializer.SerializeAsync(gzip, item);

        string key = $"{item.Type}:{item.Id}";
        _cache.Set(key, item, TimeSpan.FromMinutes(5));
    }

    public async Task<ItemEnvelope?> GetItemAsync(string type, string id)
    {
        string key = $"{type}:{id}";
        if (_cache.TryGetValue(key, out ItemEnvelope cached))
            return cached;

        string filePath = GetFilePath(type, id);
        if (!File.Exists(filePath)) return null;

        await using var fs = File.OpenRead(filePath);
        await using var gzip = new GZipStream(fs, CompressionMode.Decompress);
        var item = await JsonSerializer.DeserializeAsync<ItemEnvelope>(gzip);

        if (item != null)
            _cache.Set(key, item, TimeSpan.FromMinutes(5));

        return item;
    }

    public async Task<List<ItemEnvelope>> GetAllItemsAsync(string type, int skip = 0, int take = int.MaxValue)
    {
        string typeDir = Path.Combine(_root, type);
        if (!Directory.Exists(typeDir)) return new List<ItemEnvelope>();

        var list = new List<ItemEnvelope>();
        var files = Directory.EnumerateFiles(typeDir, "*.json.gz", SearchOption.AllDirectories)
                             .Skip(skip)
                             .Take(take);

        foreach (var f in files)
        {
            await using var fs = File.OpenRead(f);
            await using var gzip = new GZipStream(fs, CompressionMode.Decompress);
            var item = await JsonSerializer.DeserializeAsync<ItemEnvelope>(gzip);
            if (item != null) list.Add(item);
        }

        return list;
    }

    public Task<bool> DeleteItemAsync(string type, string id)
    {
        string filePath = GetFilePath(type, id);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            string key = $"{type}:{id}";
            _cache.Remove(key);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}
