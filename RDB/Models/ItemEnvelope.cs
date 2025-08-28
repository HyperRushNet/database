using System;
using System.Text.Json;

namespace RDB.Models
{
    public class ItemEnvelope
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("n");
        public string Type { get; set; } = "";
        public string Path { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public JsonElement Payload { get; set; }
        public string ToJson() => JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = false });
    }
}
