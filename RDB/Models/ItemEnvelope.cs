using System;
using System.Collections.Generic;

namespace RDB.Models
{
    public class ItemEnvelope
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public Dictionary<string, object> Payload { get; set; }
        public string RelativePath { get; set; }
        public long SizeBytes { get; set; }

        public ItemEnvelope()
        {
            Id = Guid.NewGuid().ToString("N");
            CreatedAt = DateTime.UtcNow;
            Payload = new Dictionary<string, object>();
        }
    }
}
