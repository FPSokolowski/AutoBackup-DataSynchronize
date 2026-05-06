// See https://aka.ms/new-console-template for more information
using ABDS.SharedIpc;

if (args.Length == 0)
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  ABDS.Cli.exe sync --all [--open-gui]");
    Console.WriteLine("  ABDS.Cli.exe backup --all [--open-gui]");
    return 1;
}

var cmd0 = args[0].ToLowerInvariant();
var openGui = args.Any(a => a.Equals("--open-gui", StringComparison.OrdinalIgnoreCase));
var all = args.Any(a => a.Equals("--all", StringComparison.OrdinalIgnoreCase));

var ipc = new AbdsIpcClient();

AbdsCommand cmd = cmd0 switch
{
    "sync" when all => new AbdsCommand(AbdsCommandType.ForceSyncAll),
    "backup" when all => new AbdsCommand(AbdsCommandType.ForceBackupAll),
    _ => new AbdsCommand(AbdsCommandType.GetStatus)
};

var res = await ipc.SendAsync(cmd);
Console.WriteLine(res.Ok ? "OK" : "FAIL");
Console.WriteLine(res.Message);
if (res.RunId is not null)
    Console.WriteLine($"RunId={res.RunId}");

if (openGui && res.RunId is not null)
{
    // 1) powiedz service żeby “poprosił GUI o otwarcie” (soft)
    await ipc.SendAsync(new AbdsCommand(AbdsCommandType.OpenGui, new() { ["runId"] = res.RunId }));

    // 2) hard: preferuj desktopowy host WebView2, fallback do lokalnego URL
    var url = $"http://localhost:5076/?runId={Uri.EscapeDataString(res.RunId)}";
    var desktopHost = Path.Combine(AppContext.BaseDirectory, "ABDS.DesktopHost.exe");
    try
    {
        if (File.Exists(desktopHost))
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = desktopHost,
                Arguments = $"--runId {res.RunId}",
                UseShellExecute = true
            });
        }
        else
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
    }
    catch
    {
        Console.WriteLine($"Open GUI manually: {url}");
    }
}

return res.Ok ? 0 : 2;
