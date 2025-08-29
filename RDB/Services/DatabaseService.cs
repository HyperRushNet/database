using LiteDB;
using RDB.Models;

namespace RDB.Services;

public class DatabaseService : IStorageService, IDisposable
{
    private readonly LiteDatabase _db;

    public DatabaseService()
    {
        var dataDir = Environment.GetEnvironmentVariable("DATA_DIR") 
                      ?? Path.Combine(AppContext.BaseDirectory, "data");
        Directory.CreateDirectory(dataDir);

        var dbPath = Path.Combine(dataDir, "rdb.db");
        _db = new LiteDatabase($"Filename={dbPath};Connection=shared");
    }

    public Task SaveItemAsync(ItemEnvelope item)
    {
        try
        {
            var col = _db.GetCollection<ItemEnvelope>(item.Type);
            col.Upsert(item);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving item: {ex}");
            throw;
        }
        return Task.CompletedTask;
    }

    public Task<ItemEnvelope?> GetItemAsync(string type, string id)
    {
        try
        {
            var col = _db.GetCollection<ItemEnvelope>(type);
            return Task.FromResult(col.FindById(id));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting item: {ex}");
            return Task.FromResult<ItemEnvelope?>(null);
        }
    }

    public Task<List<ItemEnvelope>> GetAllItemsAsync(string type, int skip = 0, int take = 100)
    {
        try
        {
            var col = _db.GetCollection<ItemEnvelope>(type);
            var items = col.FindAll().Skip(skip).Take(take).ToList();
            return Task.FromResult(items);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting all items: {ex}");
            return Task.FromResult(new List<ItemEnvelope>());
        }
    }

    public Task<bool> DeleteItemAsync(string type, string id)
    {
        try
        {
            var col = _db.GetCollection<ItemEnvelope>(type);
            return Task.FromResult(col.Delete(id));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting item: {ex}");
            return Task.FromResult(false);
        }
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
