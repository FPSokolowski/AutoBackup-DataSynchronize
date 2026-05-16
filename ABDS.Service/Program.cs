using ABDS.Core.Hashing;
using ABDS.Service;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5077, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

builder.Services.AddWindowsService(o =>
{
    o.ServiceName = "ABDS (AutoBackup & DataSynchronize)";
});

builder.Services.AddGrpc();

builder.Services.AddSingleton<IHashCache>(sp =>
{
    var paths = AbdsPaths.Default();
    var cache = new JsonHashCache(
        filePath: paths.HashCachePath,
        maxEntries: 150_000,
        maxAge: TimeSpan.FromDays(30));

    return cache;
});

builder.Services.AddSingleton<AbdsStateStore>();
builder.Services.AddSingleton<AbdsRunner>();
builder.Services.AddSingleton<AbdsWorkerFacade>();
builder.Services.AddSingleton<AbdsIpcServer>();
builder.Services.AddHostedService<AbdsWorker>();

var app = builder.Build();
app.MapGrpcService<AbdsIpcServer>();
await app.RunAsync();
