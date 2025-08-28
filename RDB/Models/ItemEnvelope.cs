namespace RDB.Models;

public class ItemEnvelope
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public object Payload { get; set; } = new {};
    public string RelativePath { get; set; } = string.Empty;
    public long SizeBytes { get; set; } = 0;
}
