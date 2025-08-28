using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateSlimBuilder(args);
var app = builder.Build();

var dataDir = "/data";
Directory.CreateDirectory(dataDir);

var queue = new ConcurrentQueue<string>();
var locker = new object();
var nextChunkId = GetNextChunkId(dataDir);
var timer = new Timer(FlushQueue, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

app.MapGet("/", () => Results.Ok("Healthy"));

app.MapGet("/items", () =>
{
    var items = new List<string>();
    foreach (var file in Directory.GetFiles(dataDir, "*.txt").OrderBy(f => int.Parse(Path.GetFileNameWithoutExtension(f))))
    {
        items.AddRange(File.ReadAllLines(file));
    }
    return Results.Ok(items);
});

app.MapPost("/items", async (HttpContext context) =>
{
    var item = await new StreamReader(context.Request.Body).ReadToEndAsync();
    if (!string.IsNullOrEmpty(item))
    {
        queue.Enqueue(item);
    }
    return Results.Accepted();
});

app.MapPost("/flush", () =>
{
    FlushQueue(null);
    return Results.Ok();
});

app.Run("http://0.0.0.0:10000");

void FlushQueue(object? state)
{
    if (queue.IsEmpty) return;

    lock (locker)
    {
        if (queue.IsEmpty) return;

        var chunkFile = Path.Combine(dataDir, $"{nextChunkId++}.txt");
        using (var writer = new StreamWriter(chunkFile, append: true))
        {
            while (queue.TryDequeue(out var item))
            {
                writer.WriteLine(item);
            }
        }
    }
}

int GetNextChunkId(string dir)
{
    var files = Directory.GetFiles(dir, "*.txt");
    if (files.Length == 0) return 1;
    return files.Max(f => int.Parse(Path.GetFileNameWithoutExtension(f))) + 1;
}

// Handle async exceptions safely
AppDomain.CurrentDomain.UnhandledException += (sender, e) => Console.WriteLine(e.ExceptionObject);
TaskScheduler.UnobservedTaskException += (sender, e) =>
{
    Console.WriteLine(e.Exception);
    e.SetObserved();
};
