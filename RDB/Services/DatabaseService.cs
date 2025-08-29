using LiteDB;
using RDB.Models;

namespace RDB.Services;

public class DatabaseService : IStorageService, IDisposable
{
    private readonly LiteDatabase _db;

    public DatabaseService()
    {
        var dataDir = Environment.GetEnvironmentVariable("DATA_DIR") ?? Path.Combine(AppContext.BaseDirectory, "data");
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
        try
        {
            var col = _db.GetCollection(type);
            var doc = col.FindById(id);
            if (doc == null) return Task.FromResult<ItemEnvelope?>(null);

            var item = BsonMapper.Global.ToObject<ItemEnvelope>(doc);
            return Task.FromResult(item);
        }
        catch
        {
            return Task.FromResult<ItemEnvelope?>(null);
        }
    }

    public Task<List<ItemEnvelope>> GetAllItemsAsync(string type, int skip = 0, int take = 100)
    {
        var safeList = new List<ItemEnvelope>();

        try
        {
            var col = _db.GetCollection(type);
            foreach (var doc in col.FindAll())
            {
                try
                {
                    var item = BsonMapper.Global.ToObject<ItemEnvelope>(doc);
                    safeList.Add(item);
                }
                catch
                {
                    // Negeer corrupte item
                }
            }
        }
        catch
        {
            // Hele collectie kan corrupt zijn, retourneer lege lijst
        }

        return Task.FromResult(safeList.Skip(skip).Take(take).ToList());
    }

    public Task<bool> DeleteItemAsync(string type, string id)
    {
        try
        {
            var col = _db.GetCollection<ItemEnvelope>(type);
            return Task.FromResult(col.Delete(id));
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
