using RDB.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IStorageService, DatabaseService>();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("UniversalCors", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("UniversalCors");
app.MapControllers();

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://*:{port}");

app.Run();
