using ABDS.Core.Hashing;
using ABDS.Service;
using Microsoft.Extensions.Hosting.WindowsServices;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(o =>
{
    o.ServiceName = "ABDS (AutoBackup & DataSynchronize)";
});

// ---------- HASH CACHE SINGLETON ----------
builder.Services.AddSingleton<IHashCache>(sp =>
{
    var paths = AbdsPaths.Default();
    var cache = new JsonHashCache(
        filePath: paths.HashCachePath,
        maxEntries: 150_000,
        maxAge: TimeSpan.FromDays(30));

    return cache;
});
// ------------------------------------------

builder.Services.AddSingleton<AbdsStateStore>();
builder.Services.AddSingleton<AbdsRunner>();
builder.Services.AddSingleton<AbdsWorkerFacade>();
builder.Services.AddSingleton<AbdsIpcServer>();
builder.Services.AddHostedService<AbdsWorker>();

var host = builder.Build();
await host.RunAsync();
