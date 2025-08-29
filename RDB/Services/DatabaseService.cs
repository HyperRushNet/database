using LiteDB;
using RDB.Models;

namespace RDB.Services;

public class DatabaseService : IStorageService, IDisposable
{
    private readonly LiteDatabase _db;

    public DatabaseService()
    {
        var dataDir = Environment.GetEnvironmentVariable("DATA_DIR") ?? "/tmp/data";
        Directory.CreateDirectory(dataDir);

        var dbPath = Path.Combine(dataDir, "rdb.db");
        _db = new LiteDatabase($"Filename={dbPath};Connection=shared");
    }

    public Task SaveItemAsync(ItemEnvelope item)
    {
        var col = _db.GetCollection<ItemEnvelope>(item.Type);
        col.Upsert(item);
        return Task.CompletedTask;
    }

    public Task<ItemEnvelope?> GetItemAsync(string type, string id)
    {
        var col = _db.GetCollection<ItemEnvelope>(type);
        return Task.FromResult(col.FindById(id));
    }

    public Task<List<ItemEnvelope>> GetAllItemsAsync(string type, int skip = 0, int take = 100)
    {
        var col = _db.GetCollection<ItemEnvelope>(type);
        var items = col.FindAll().Skip(skip).Take(take).ToList();
        return Task.FromResult(items);
    }

    public Task<bool> DeleteItemAsync(string type, string id)
    {
        var col = _db.GetCollection<ItemEnvelope>(type);
        return Task.FromResult(col.Delete(id));
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
