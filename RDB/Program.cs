using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using RDB.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();
app.UseCors();

app.MapPost("/database", async (HttpRequest req, DatabaseService db) =>
{
    var type = req.Query["type"].ToString();
    if(string.IsNullOrWhiteSpace(type)) return Results.BadRequest();
    var payload = await JsonSerializer.DeserializeAsync<object>(req.Body) ?? new {};
    var item = db.AddItem(type, payload);
    return Results.Json(new { id = item.Id });
});

app.MapGet("/database/item", (HttpRequest req, DatabaseService db) =>
{
    var type = req.Query["type"].ToString();
    var id = req.Query["id"].ToString();
    if(string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(id)) return Results.BadRequest();
    var item = db.GetItem(type, id);
    if(item==null) return Results.StatusCode(204);
    return Results.Json(item);
});

app.MapGet("/database/items", (HttpRequest req, DatabaseService db) =>
{
    var type = req.Query["type"].ToString();
    if(string.IsNullOrWhiteSpace(type)) return Results.BadRequest();
    int skip = int.TryParse(req.Query["skip"], out var s) ? s : 0;
    int limit = int.TryParse(req.Query["limit"], out var l) ? l : 50;

    var filters = req.Query
        .Where(q => q.Key != "type" && q.Key != "skip" && q.Key != "limit")
        .ToDictionary(kv => kv.Key, kv => kv.Value.ToString());

    var items = db.GetItems(type, skip, limit, filters.Count>0?filters:null);
    return Results.Json(items);
});

app.MapDelete("/database/item", (HttpRequest req, DatabaseService db) =>
{
    var type = req.Query["type"].ToString();
    var id = req.Query["id"].ToString();
    if(string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(id)) return Results.BadRequest();
    var success = db.DeleteItem(type, id);
    return Results.Json(new { deleted = success });
});

app.MapGet("/database/types", (DatabaseService db) =>
{
    var types = db.GetAllTypes();
    return Results.Json(types);
});

app.Use(async (context, next) =>
{
    context.Response.Headers["Access-Control-Allow-Origin"] = "*";
    context.Response.Headers["Access-Control-Allow-Headers"] = "*";
    context.Response.Headers["Access-Control-Allow-Methods"] = "*";
    await next();
});

app.Run(async context =>
{
    context.Response.StatusCode = 204;
    await context.Response.CompleteAsync();
});

app.Run();
