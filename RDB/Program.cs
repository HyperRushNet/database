using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RDB.Services;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddResponseCompression();
builder.Services.AddCors(p => p.AddDefaultPolicy(b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

app.UseCors();
app.UseResponseCompression();

if (!Directory.Exists("data")) Directory.CreateDirectory("data");

app.Use(async (context, next) =>
{
    try
    {
        await next();
        if (context.Response.StatusCode == 404)
        {
            context.Response.StatusCode = 0;
            await context.Response.Body.FlushAsync();
        }
    }
    catch
    {
        context.Response.StatusCode = 0;
        await context.Response.Body.FlushAsync();
    }
});

app.MapPost("/database", async (DatabaseService db, HttpRequest req) =>
{
    var type = req.Query["type"].ToString();
    if (string.IsNullOrEmpty(type)) return Results.StatusCode(0);

    using var reader = new StreamReader(req.Body);
    var body = await reader.ReadToEndAsync();
    object payload;
    try
    {
        payload = body.Length > 0 ? JsonSerializer.Deserialize<object>(body) ?? new {} : new {};
    }
    catch
    {
        payload = new {};
    }

    var item = db.AddItem(type, payload);
    return Results.Json(new { id = item.Id });
});

app.MapGet("/database/item", (DatabaseService db, string type, string id, bool raw = false) =>
{
    var item = db.GetItem(type, id);
    if (item == null) return Results.StatusCode(0);
    if (raw) return Results.Json(item);
    return Results.Json(new { item.Id });
});

app.MapGet("/database/items", (DatabaseService db, string type) =>
{
    var items = db.GetItems(type).Select(i => new { i.Id });
    return Results.Json(items);
});

app.MapDelete("/database/item", (DatabaseService db, string type, string id) =>
{
    var success = db.DeleteItem(type, id);
    return success ? Results.Json(new { id }) : Results.StatusCode(0);
});

app.MapFallback(async context =>
{
    context.Response.StatusCode = 0;
    await context.Response.Body.FlushAsync();
});

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://*:{port}");
app.Run();
