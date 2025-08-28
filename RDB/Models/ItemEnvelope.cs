using System;
using System.Collections.Generic;

namespace RDB.Models
{
    public class ItemEnvelope
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string RelativePath { get; set; } = string.Empty;
        public Dictionary<string, object> Payload { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
