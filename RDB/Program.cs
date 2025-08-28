using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using RDB.Models;
using RDB.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
var app = builder.Build();
app.UseCors();

app.MapPost("/database", async (HttpContext ctx, DatabaseService db) =>
{
    var type = ctx.Request.Query["type"].ToString();
    if (string.IsNullOrWhiteSpace(type)) { ctx.Response.StatusCode = 400; return; }

    var payload = await JsonSerializer.DeserializeAsync<Dictionary<string, object>>(ctx.Request.Body);
    var item = await db.AddItem(type, payload ?? new Dictionary<string, object>());
    ctx.Response.ContentType = "application/json";
    await ctx.Response.WriteAsync(JsonSerializer.Serialize(item));
});

app.MapGet("/database/items", async (HttpContext ctx, DatabaseService db) =>
{
    var type = ctx.Request.Query["type"].ToString();
    if (string.IsNullOrWhiteSpace(type)) { ctx.Response.StatusCode = 400; return; }

    var items = await db.GetItems(type);
    ctx.Response.ContentType = "application/json";
    await ctx.Response.WriteAsync(JsonSerializer.Serialize(items));
});

app.MapGet("/database/item", async (HttpContext ctx, DatabaseService db) =>
{
    var type = ctx.Request.Query["type"].ToString();
    var id = ctx.Request.Query["id"].ToString();
    if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(id)) { ctx.Response.StatusCode = 400; return; }

    var item = await db.GetItem(type, id);
    if (item == null) { ctx.Response.StatusCode = 404; return; }
    ctx.Response.ContentType = "application/json";
    await ctx.Response.WriteAsync(JsonSerializer.Serialize(item));
});

app.MapDelete("/database/item", async (HttpContext ctx, DatabaseService db) =>
{
    var type = ctx.Request.Query["type"].ToString();
    var id = ctx.Request.Query["id"].ToString();
    if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(id)) { ctx.Response.StatusCode = 400; return; }

    var deleted = await db.DeleteItem(type, id);
    ctx.Response.StatusCode = deleted ? 200 : 404;
});

app.Run(async ctx =>
{
    ctx.Response.StatusCode = 404;
});

app.Run();
