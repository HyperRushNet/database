using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RDB.Services;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddCors(p => p.AddDefaultPolicy(b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();
app.UseCors();

if (!Directory.Exists("data")) Directory.CreateDirectory("data");

// Get all types
app.MapGet("/database/types", () =>
{
    if (!Directory.Exists("data")) return Results.Json(Array.Empty<string>());
    var types = Directory.GetDirectories("data").Select(d => Path.GetFileName(d));
    return Results.Json(types);
});

// Add item
app.MapPost("/database", async (DatabaseService db, HttpRequest req) =>
{
    var type = req.Query["type"].ToString();
    if (string.IsNullOrEmpty(type)) return Results.StatusCode(204);
    using var reader = new StreamReader(req.Body);
    var body = await reader.ReadToEndAsync();
    object payload = new {};
    try { payload = body.Length>0 ? JsonSerializer.Deserialize<object>(body) ?? new {} : new {}; } catch {}
    var item = db.AddItem(type, payload);
    return Results.Json(item); // returns full item, raw JSON
});

// Get single item
app.MapGet("/database/item", (DatabaseService db, string type, string id) =>
{
    var item = db.GetItem(type, id);
    return item == null ? Results.StatusCode(204) : Results.Json(item);
});

// List items
app.MapGet("/database/items", (DatabaseService db, string type) =>
{
    var items = db.GetItems(type);
    return items.Count == 0 ? Results.StatusCode(204) : Results.Json(items);
});

// Delete item
app.MapDelete("/database/item", (DatabaseService db, string type, string id) =>
{
    var success = db.DeleteItem(type, id);
    return success ? Results.StatusCode(204) : Results.StatusCode(204);
});

// Edit item
app.MapPut("/database/item", async (DatabaseService db, HttpRequest req) =>
{
    var type = req.Query["type"].ToString();
    var id = req.Query["id"].ToString();
    using var reader = new StreamReader(req.Body);
    var body = await reader.ReadToEndAsync();
    object payload = new {};
    try { payload = body.Length>0 ? JsonSerializer.Deserialize<object>(body) ?? new {} : new {}; } catch {}
    var item = db.UpdateItem(type, id, payload);
    return item == null ? Results.StatusCode(204) : Results.Json(item);
});

app.Run();
