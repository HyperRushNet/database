using System.Collections.Concurrent;
using RDB.Models;

namespace RDB.Services;

public class DatabaseService : IStorageService
{
    private readonly ConcurrentDictionary<string, ItemEnvelope> _items = new();

    public Task SaveItemAsync(ItemEnvelope item)
    {
        _items[item.Id] = item;
        return Task.CompletedTask;
    }

    public Task<ItemEnvelope?> GetItemAsync(string type, string id)
    {
        _items.TryGetValue(id, out var item);
        if (item != null && item.Type == type) return Task.FromResult(item);
        return Task.FromResult<ItemEnvelope?>(null);
    }

    public Task<List<ItemEnvelope>> GetAllItemsAsync(string type)
    {
        var list = _items.Values.Where(x => x.Type == type).ToList();
        return Task.FromResult(list);
    }
}
