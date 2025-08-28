using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RDB.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddCors(p => p.AddDefaultPolicy(b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

app.UseCors();

app.MapControllers();
app.MapFallback(context => { context.Response.StatusCode = 204; return Task.CompletedTask; });

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://*:{port}");
app.Run();
