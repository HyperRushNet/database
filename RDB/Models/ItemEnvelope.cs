using System;

namespace RDB.Models
{
    public class ItemEnvelope
    {
        public string Id { get; set; } = "";
        public string Type { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public object Payload { get; set; } = new {};
        public string RelativePath { get; set; } = "";
        public long SizeBytes { get; set; }
    }
}
