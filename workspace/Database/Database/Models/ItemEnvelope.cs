using System;
using System.Text.Json;

namespace Database.Models
{
    public class ItemEnvelope
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("n");
        public string Type { get; set; } = "";
        public string Path { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public JsonElement Payload { get; set; }

        public string ToJson()
        {
            var opts = new JsonSerializerOptions { WriteIndented = false };
            return JsonSerializer.Serialize(this, opts);
        }
    }
}
