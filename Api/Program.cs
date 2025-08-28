using System.Collections.Concurrent;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

const string DataDir = "data";
Directory.CreateDirectory(DataDir);

var queue = new ConcurrentQueue<string>();
var flushLock = new SemaphoreSlim(1, 1);
var flushInterval = TimeSpan.FromMinutes(1);

// Health check
app.MapGet("/", () => Results.Ok("API is running"));

// Get all items from chunk files
app.MapGet("/items", async () =>
{
    var allItems = new List<string>();
    var files = Directory.GetFiles(DataDir, "*.txt").OrderBy(f => f);
    foreach (var file in files)
    {
        var lines = await File.ReadAllLinesAsync(file);
        allItems.AddRange(lines);
    }
    return Results.Ok(allItems);
});

// Add item to in-memory queue
app.MapPost("/items", async (HttpRequest request) =>
{
    using var reader = new StreamReader(request.Body);
    var item = await reader.ReadToEndAsync();
    if (!string.IsNullOrWhiteSpace(item))
    {
        queue.Enqueue(item.Trim());
        return Results.Ok("Item queued");
    }
    return Results.BadRequest("Empty item");
});

// Flush queue to new chunk file
app.MapPost("/flush", async () =>
{
    await FlushQueueAsync();
    return Results.Ok("Queue flushed");
});

// Automatic flush timer
var timer = new PeriodicTimer(flushInterval);
_ = Task.Run(async () =>
{
    while (await timer.WaitForNextTickAsync())
    {
        try
        {
            await FlushQueueAsync();
        }
        catch
        {
            // safe for async exceptions
        }
    }
});

app.Run("http://0.0.0.0:10000");

async Task FlushQueueAsync()
{
    if (!queue.Any()) return;

    await flushLock.WaitAsync();
    try
    {
        var itemsToFlush = new List<string>();
        while (queue.TryDequeue(out var item))
        {
            itemsToFlush.Add(item);
        }

        if (!itemsToFlush.Any()) return;

        int nextFileNumber = 1;
        var existingFiles = Directory.GetFiles(DataDir, "*.txt")
                                     .Select(f => Path.GetFileNameWithoutExtension(f))
                                     .Select(f => int.TryParse(f, out var n) ? n : 0)
                                     .Where(n => n > 0)
                                     .ToList();
        if (existingFiles.Any())
        {
            nextFileNumber = existingFiles.Max() + 1;
        }

        var filePath = Path.Combine(DataDir, $"{nextFileNumber}.txt");
        await File.WriteAllLinesAsync(filePath, itemsToFlush, Encoding.UTF8);
    }
    finally
    {
        flushLock.Release();
    }
}
