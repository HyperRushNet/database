using System;

namespace Database.Models
{
    public class IndexEntry
    {
        public string Id { get; set; } = "";
        public string Type { get; set; } = "";
        public string RelativePath { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public long SizeBytes { get; set; } = 0;
    }
}
