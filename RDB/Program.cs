using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using RDB.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<DatabaseService>();
builder.Services.Configure<JsonOptions>(o => o.SerializerOptions.PropertyNamingPolicy = null);

var app = builder.Build();
app.UseCors(cors => cors.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.MapPost("/database", async (string type, DatabaseService db, HttpContext ctx) =>
{
    var raw = await ctx.Request.ReadFromJsonAsync<object>();
    var payload = raw is JsonElement je
        ? JsonSerializer.Deserialize<Dictionary<string, object>>(je.GetRawText()) ?? new Dictionary<string, object>()
        : new Dictionary<string, object>();
    var item = await db.AddItem(type, payload);
    return Results.Json(new { id = item.Id });
});

app.MapGet("/database/items", async (string type, DatabaseService db) =>
{
    var items = await db.GetItems(type);
    return Results.Json(items);
});

app.MapGet("/database/item", async (string type, string id, DatabaseService db) =>
{
    var item = await db.GetItem(type, id);
    return item is null ? Results.StatusCode(404) : Results.Json(item);
});

app.MapDelete("/database/item", async (string type, string id, DatabaseService db) =>
{
    var ok = await db.DeleteItem(type, id);
    return ok ? Results.Ok() : Results.StatusCode(404);
});

app.Run();
