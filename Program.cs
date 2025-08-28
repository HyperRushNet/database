using System.Collections.Concurrent;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Data folder
var dataFolder = Path.Combine(AppContext.BaseDirectory, "data");
if (!Directory.Exists(dataFolder)) Directory.CreateDirectory(dataFolder);

// In-memory queue voor batching
var batchQueue = new ConcurrentQueue<string>();
var currentChunk = GetLastChunkNumber(dataFolder) + 1;
var maxItemsPerChunk = 1000; // pas aan naar wens

// Helper: laatste chunk number
int GetLastChunkNumber(string folder)
{
    var files = Directory.GetFiles(folder, "*.txt");
    if (!files.Any()) return 0;
    return files.Select(f => int.Parse(Path.GetFileNameWithoutExtension(f))).Max();
}

// Flush in-memory queue naar chunk file
async Task FlushQueue()
{
    if (batchQueue.IsEmpty) return;

    var items = new List<string>();
    while (items.Count < maxItemsPerChunk && batchQueue.TryDequeue(out var item))
        items.Add(item);

    if (items.Count == 0) return;

    var filename = Path.Combine(dataFolder, $"{currentChunk}.txt");
    await File.WriteAllLinesAsync(filename, items, Encoding.UTF8);
    currentChunk++;
}

// Background timer flush
var flushTimer = new System.Threading.Timer(async _ => await FlushQueue(), null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

// Routes

app.MapGet("/", () => Results.Text("Hello Render Free chunked file API!"));

app.MapGet("/items", () =>
{
    var allFiles = Directory.GetFiles(dataFolder, "*.txt").OrderBy(f => f);
    var allItems = new List<string>();
    foreach (var file in allFiles)
    {
        allItems.AddRange(File.ReadAllLines(file, Encoding.UTF8));
    }
    return Results.Ok(allItems);
});

app.MapPost("/items", (ItemCreate req) =>
{
    if (string.IsNullOrWhiteSpace(req.Data)) return Results.BadRequest("Data required");
    batchQueue.Enqueue(req.Data);
    return Results.Accepted();
});

// Optioneel: flush handmatig
app.MapPost("/flush", async () =>
{
    await FlushQueue();
    return Results.Ok("Flushed batch queue.");
});

app.Run();

record ItemCreate(string Data);
