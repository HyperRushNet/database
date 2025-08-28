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
File.SetAttributes(dataDir, FileAttributes.Normal); // Zorg voor schrijfrechten

var queue = new ConcurrentQueue<string>();
var locker = new object();
var nextChunkId = GetNextChunkId(dataDir);
var timer = new Timer(FlushQueue, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

app.MapGet("/", () => Results.Ok("Healthy"));

app.MapGet("/items", () =>
{
    var items = new List<string>();
    try
    {
        foreach (var file in Directory.GetFiles(dataDir, "*.txt").OrderBy(f => int.Parse(Path.GetFileNameWithoutExtension(f))))
        {
            items.AddRange(File.ReadAllLines(file));
        }
        return Results.Ok(items);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Fout bij /items: {ex}");
        File.AppendAllText("/data/error.log", $"[{DateTime.UtcNow}] Fout bij /items: {ex}\n");
        return Results.StatusCode(500);
    }
});

app.MapPost("/items", async (HttpContext context) =>
{
    try
    {
        var item = await new StreamReader(context.Request.Body).ReadToEndAsync();
        if (!string.IsNullOrEmpty(item))
        {
            queue.Enqueue(item);
            return Results.Accepted();
        }
        return Results.BadRequest("Item mag niet leeg zijn");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Fout bij POST /items: {ex}");
        File.AppendAllText("/data/error.log", $"[{DateTime.UtcNow}] Fout bij POST /items: {ex}\n");
        return Results.StatusCode(500);
    }
});

app.MapPost("/flush", () =>
{
    try
    {
        FlushQueue(null);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Fout bij /flush: {ex}");
        File.AppendAllText("/data/error.log", $"[{DateTime.UtcNow}] Fout bij /flush: {ex}\n");
        return Results.StatusCode(500);
    }
});

app.Run("http://0.0.0.0:10000");

void FlushQueue(object? state)
{
    if (queue.IsEmpty) return;

    lock (locker)
    {
        if (queue.IsEmpty) return;

        try
        {
            var chunkFile = Path.Combine(dataDir, $"{nextChunkId++}.txt");
            using (var writer = new StreamWriter(chunkFile, append: true))
            {
                while (queue.TryDequeue(out var item))
                {
                    writer.WriteLine(item);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fout bij FlushQueue: {ex}");
            File.AppendAllText("/data/error.log", $"[{DateTime.UtcNow}] Fout bij FlushQueue: {ex}\n");
        }
    }
}

int GetNextChunkId(string dir)
{
    try
    {
        var files = Directory.GetFiles(dir, "*.txt");
        if (files.Length == 0) return 1;
        return files.Max(f => int.Parse(Path.GetFileNameWithoutExtension(f))) + 1;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Fout bij GetNextChunkId: {ex}");
        File.AppendAllText("/data/error.log", $"[{DateTime.UtcNow}] Fout bij GetNextChunkId: {ex}\n");
        return 1;
    }
}

// Verbeterde uitzonderingsafhandeling
AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
{
    Console.WriteLine($"Onbehandelde uitzondering: {e.ExceptionObject}");
    File.AppendAllText("/data/error.log", $"[{DateTime.UtcNow}] Onbehandelde uitzondering: {e.ExceptionObject}\n");
};

TaskScheduler.UnobservedTaskException += (sender, e) =>
{
    Console.WriteLine($"Onbehandelde taakuitzondering: {e.Exception}");
    File.AppendAllText("/data/error.log", $"[{DateTime.UtcNow}] Onbehandelde taakuitzondering: {e.Exception}\n");
    e.SetObserved();
};
