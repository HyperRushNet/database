namespace RDB.Models;

public class ItemEnvelope
{
    public string Id { get; set; } = null!;
    public string Type { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public object? Payload { get; set; }
}
