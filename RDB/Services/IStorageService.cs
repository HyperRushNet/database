using RDB.Models;

namespace RDB.Services;

public interface IStorageService
{
    Task SaveItemAsync(ItemEnvelope item);
    Task<ItemEnvelope?> GetItemAsync(string type, string id);
    Task<List<ItemEnvelope>> GetAllItemsAsync(string type);
}
