using RDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDB.Services
{
    public class DatabaseService
    {
        // Simuleer de database met een dictionary per type
        private readonly Dictionary<string, List<ItemEnvelope>> _db = new();

        // Haal alle items van een type
        public Task<List<ItemEnvelope>> GetAllItems(string type)
        {
            if (!_db.ContainsKey(type))
                _db[type] = new List<ItemEnvelope>();
            return Task.FromResult(_db[type]);
        }

        // Haal een specifiek item op
        public Task<ItemEnvelope?> GetItem(string type, string id)
        {
            if (!_db.ContainsKey(type)) return Task.FromResult<ItemEnvelope?>(null);
            var item = _db[type].FirstOrDefault(i => i.Id == id);
            return Task.FromResult(item);
        }

        // Voeg een item toe
        public Task<ItemEnvelope> AddItem(string type, Dictionary<string, object> payload)
        {
            if (!_db.ContainsKey(type))
                _db[type] = new List<ItemEnvelope>();

            var item = new ItemEnvelope
            {
                Id = Guid.NewGuid().ToString(),
                Type = type,
                RelativePath = payload.ContainsKey("RelativePath") ? payload["RelativePath"].ToString() ?? "" : "",
                Payload = payload,
                CreatedAt = DateTime.UtcNow
            };
            _db[type].Add(item);
            return Task.FromResult(item);
        }

        // Verwijder een item
        public Task<bool> RemoveItem(string type, string id)
        {
            if (!_db.ContainsKey(type)) return Task.FromResult(false);
            var item = _db[type].FirstOrDefault(i => i.Id == id);
            if (item == null) return Task.FromResult(false);

            _db[type].Remove(item);
            return Task.FromResult(true);
        }
    }
}
