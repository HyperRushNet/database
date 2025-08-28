using RDB.Models;

namespace RDB.Services;

public interface IStorageService
{
    ItemEnvelope AddItem(string type, object payload);
    ItemEnvelope? GetItem(string type, string id);
    IEnumerable<ItemEnvelope> GetItems(string type);
    bool DeleteItem(string type, string id);
}
