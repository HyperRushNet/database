using RDB.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IStorageService, DatabaseService>();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors();
app.MapControllers();

// Gebruik dynamische port van Render
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://*:{port}");

app.Run();
