using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Database.Services;
using System;

var builder = WebApplication.CreateBuilder(args);

var dataDir = Environment.GetEnvironmentVariable("DATA_DIR") ?? "data";

builder.Services.AddSingleton<DatabaseService>(sp => new DatabaseService(dataDir, batchFlushThreshold: 8, batchFlushInterval: TimeSpan.FromSeconds(3)));
builder.Services.AddSingleton<IStorageService>(sp => sp.GetRequiredService<DatabaseService>());
builder.Services.AddSingleton<BatchFlushService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<BatchFlushService>());

builder.Services.AddControllers();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseEndpoints(endpoints => endpoints.MapControllers());

app.Run();
