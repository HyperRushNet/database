using System.Collections.Generic;
using System.Threading.Tasks;
using RDB.Models;
using System.Text.Json;

namespace RDB.Services
{
    public interface IStorageService
    {
        Task<IndexEntry> StoreItemAsync(string type, JsonElement payload);
        Task<ItemEnvelope?> GetItemAsync(string type, string id);
        Task<IEnumerable<IndexEntry>> ListTypeAsync(string type, int skip = 0, int take = 100);
        Task<IEnumerable<IndexEntry>> StoreBatchAsync(string type, IEnumerable<JsonElement> payloads);
    }
}
