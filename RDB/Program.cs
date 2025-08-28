using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RDB.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseCors();

app.MapGet("/database/items", async (string type, DatabaseService db) => await db.ListItems(type));
app.MapGet("/database/item", async (string type, string id, DatabaseService db) => await db.GetItem(type, id));
app.MapPost("/database", async (string type, DatabaseService db, HttpContext ctx) =>
{
    var payload = await ctx.Request.ReadFromJsonAsync<Dictionary<string, object>>();
    return await db.AddItem(type, payload);
});
app.MapDelete("/database/item", async (string type, string id, DatabaseService db) => await db.DeleteItem(type, id));
app.MapGet("/database/types", (DatabaseService db) => db.ListTypes());

app.Run(async context => { context.Response.StatusCode = 404; });

app.Run();
