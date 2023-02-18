using intStripsServer.Models;
using intStripsServer.Services;
using Microsoft.EntityFrameworkCore;
using NeoSmart.Caching.Sqlite;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddSqliteCache(options =>
{
    options.CachePath = builder.Configuration["CachePath"] ?? ". " + Path.DirectorySeparatorChar + "Data" + Path.DirectorySeparatorChar + "cache.sqlite";
});
builder.Services.AddDbContext<SqliteLogContext>(options =>
{
    var path = builder.Configuration["LogPath"] ??
               ". " + Path.DirectorySeparatorChar + "Data" + Path.DirectorySeparatorChar + "log.sqlite";
    options.UseSqlite($"Data Source={path}");
});
builder.Services.AddSingleton<UpdateStreamHandler>();
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });
app.MapGrpcService<FlightService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();