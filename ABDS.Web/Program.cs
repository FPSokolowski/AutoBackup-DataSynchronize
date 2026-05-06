using System.Text.Json;
using ABDS.Core.Destinations;
using ABDS.Core.Models;
using ABDS.SharedIpc;
using System.ServiceProcess;
using Microsoft.Win32;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<AbdsIpcClient>();
builder.Services.AddSingleton(AbdsWebPaths.Default());
builder.Services.AddSingleton(new AbdsServiceControl("ABDS (AutoBackup & DataSynchronize)"));

var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true
};

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/config", async (AbdsWebPaths paths, CancellationToken ct) =>
{
    var config = await LoadConfigAsync(paths, ct);
    return Results.Json(config, jsonOptions);
});

app.MapPut("/api/config", async (AbdsConfig config, AbdsWebPaths paths, CancellationToken ct) =>
{
    config = await DestinationConfigService.ProbeConfiguredDestinationsAsync(config, ct);
    await SaveConfigAsync(config, paths, jsonOptions, ct);
    return Results.Json(config, jsonOptions);
});

app.MapPost("/api/destinations/probe", async (DestinationProbeRequest request, CancellationToken ct) =>
{
    var result = await DestinationProbe.ProbeAsync(request.Location, writeTest: true, ct);
    return Results.Json(result, jsonOptions);
});

app.MapPost("/api/destinations/retest", async (DestinationProbeRequest request, AbdsWebPaths paths, CancellationToken ct) =>
{
    var config = await LoadConfigAsync(paths, ct);
    var result = await DestinationProbe.ProbeAsync(request.Location, writeTest: true, ct);
    config = DestinationConfigService.ApplyProbeResult(config, result);
    await SaveConfigAsync(config, paths, jsonOptions, ct);
    return Results.Json(new DestinationRetestResponse(result, config), jsonOptions);
});

app.MapGet("/api/diagnostics/paths", (AbdsWebPaths paths) => Results.Json(new
{
    paths.RootDir,
    paths.ConfigPath,
    paths.StatePath,
    paths.HashCachePath,
    paths.DumpsDir
}, jsonOptions));

app.MapGet("/api/service/status", (AbdsServiceControl service) =>
    Results.Json(service.GetStatus(), jsonOptions));

app.MapPost("/api/service/start", async (AbdsServiceControl service, CancellationToken ct) =>
    Results.Json(await service.StartAsync(ct), jsonOptions));

app.MapPost("/api/service/restart", async (AbdsServiceControl service, CancellationToken ct) =>
    Results.Json(await service.RestartAsync(ct), jsonOptions));

app.MapGet("/api/windows/startup", () =>
    Results.Json(WindowsStartupControl.GetStatus(), jsonOptions));

app.MapPut("/api/windows/startup", (StartupSettingRequest request) =>
    Results.Json(WindowsStartupControl.SetEnabled(request.Enabled), jsonOptions));

app.MapGet("/api/status", async (AbdsIpcClient ipc, CancellationToken ct) =>
{
    var response = await SendIpcAsync(ipc, new AbdsCommand(AbdsCommandType.GetStatus), ct);
    return ToApiResult(response);
});

app.MapGet("/api/runs/recent", async (int? take, AbdsIpcClient ipc, CancellationToken ct) =>
{
    var response = await SendIpcAsync(ipc, new AbdsCommand(
        AbdsCommandType.GetRecentRuns,
        new Dictionary<string, string> { ["take"] = (take ?? 50).ToString() }), ct);

    return ToApiResult(response);
});

app.MapGet("/api/runs/{runId}", async (string runId, AbdsIpcClient ipc, CancellationToken ct) =>
{
    var response = await SendIpcAsync(ipc, new AbdsCommand(
        AbdsCommandType.GetRunDetails,
        new Dictionary<string, string> { ["runId"] = runId }), ct);

    return ToApiResult(response);
});

app.MapGet("/api/runs/{runId}/logs", async (string runId, AbdsIpcClient ipc, CancellationToken ct) =>
{
    var response = await SendIpcAsync(ipc, new AbdsCommand(
        AbdsCommandType.GetRunLogs,
        new Dictionary<string, string> { ["runId"] = runId }), ct);

    return ToApiResult(response);
});

app.MapPost("/api/actions/sync/all", async (AbdsIpcClient ipc, CancellationToken ct) =>
{
    var response = await SendIpcAsync(ipc, new AbdsCommand(AbdsCommandType.ForceSyncAll), ct);
    return ToApiResult(response);
});

app.MapPost("/api/actions/backup/all", async (AbdsIpcClient ipc, CancellationToken ct) =>
{
    var response = await SendIpcAsync(ipc, new AbdsCommand(AbdsCommandType.ForceBackupAll), ct);
    return ToApiResult(response);
});

app.MapPost("/api/actions/sync/pair", async (SyncPairRequest request, AbdsIpcClient ipc, CancellationToken ct) =>
{
    var response = await SendIpcAsync(ipc, new AbdsCommand(
        AbdsCommandType.ForceSyncPair,
        new Dictionary<string, string>
        {
            ["sourcePath"] = request.SourcePath,
            ["targetPath"] = request.TargetPath
        }), ct);

    return ToApiResult(response);
});

app.MapPost("/api/actions/backup/source", async (BackupSourceRequest request, AbdsIpcClient ipc, CancellationToken ct) =>
{
    var response = await SendIpcAsync(ipc, new AbdsCommand(
        AbdsCommandType.ForceBackupSource,
        new Dictionary<string, string>
        {
            ["sourcePath"] = request.SourcePath,
            ["backupRootPath"] = request.BackupRootPath
        }), ct);

    return ToApiResult(response);
});

app.MapPost("/api/runs/{runId}/cancel", async (string runId, AbdsIpcClient ipc, CancellationToken ct) =>
{
    var response = await SendIpcAsync(ipc, new AbdsCommand(
        AbdsCommandType.CancelRun,
        new Dictionary<string, string> { ["runId"] = runId }), ct);

    return ToApiResult(response);
});

app.MapFallbackToFile("index.html");

await app.RunAsync();

static async Task<AbdsCommandResponse> SendIpcAsync(AbdsIpcClient ipc, AbdsCommand command, CancellationToken ct)
{
    try
    {
        return await ipc.SendAsync(command, ct);
    }
    catch (Exception ex) when (ex is System.TimeoutException or IOException or OperationCanceledException)
    {
        return new AbdsCommandResponse(false, $"Brak połączenia z usługą ABDS: {ex.Message}");
    }
}

static IResult ToApiResult(AbdsCommandResponse response)
{
    if (!response.Ok)
        return Results.Problem(response.Message, statusCode: StatusCodes.Status503ServiceUnavailable);

    if (LooksLikeJson(response.Message))
        return Results.Text(response.Message, "application/json");

    return Results.Json(response);
}

static bool LooksLikeJson(string value)
{
    var trimmed = value.TrimStart();
    return trimmed.StartsWith('{') || trimmed.StartsWith('[');
}

static async Task<AbdsConfig> LoadConfigAsync(AbdsWebPaths paths, CancellationToken ct)
{
    Directory.CreateDirectory(paths.RootDir);

    if (!File.Exists(paths.ConfigPath))
    {
        var config = new AbdsConfig();
        await SaveConfigAsync(config, paths, new JsonSerializerOptions { WriteIndented = true }, ct);
        return config;
    }

    var content = await File.ReadAllTextAsync(paths.ConfigPath, ct);
    return JsonSerializer.Deserialize<AbdsConfig>(content) ?? new AbdsConfig();
}

static async Task SaveConfigAsync(AbdsConfig config, AbdsWebPaths paths, JsonSerializerOptions options, CancellationToken ct)
{
    Directory.CreateDirectory(paths.RootDir);
    var json = JsonSerializer.Serialize(config, options);
    await File.WriteAllTextAsync(paths.ConfigPath, json, ct);
}

public sealed record AbdsWebPaths(string RootDir)
{
    public string ConfigPath => Path.Combine(RootDir, "config.json");
    public string StatePath => Path.Combine(RootDir, "state.json");
    public string DumpsDir => Path.Combine(RootDir, "Dumps");
    public string HashCachePath => Path.Combine(RootDir, "hashcache.json");

    public static AbdsWebPaths Default()
        => new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ABDS"));
}

public sealed record SyncPairRequest(string SourcePath, string TargetPath);
public sealed record BackupSourceRequest(string SourcePath, string BackupRootPath);
public sealed record DestinationProbeRequest(string Location);
public sealed record DestinationRetestResponse(DestinationProbeResult Result, AbdsConfig Config);
public sealed record StartupSettingRequest(bool Enabled);
public sealed record StartupSettingDto(bool Enabled, string RunValueName, string? TrayAgentPath, string? Message);

public sealed record AbdsServiceStatusDto(
    bool Installed,
    bool CanControl,
    string ServiceName,
    string DisplayName,
    string Status,
    string? Message);

public sealed class AbdsServiceControl(string serviceName)
{
    public AbdsServiceStatusDto GetStatus()
    {
        try
        {
            using var service = FindService();
            if (service is null)
                return NotInstalled();

            return new AbdsServiceStatusDto(
                Installed: true,
                CanControl: true,
                ServiceName: service.ServiceName,
                DisplayName: service.DisplayName,
                Status: service.Status.ToString(),
                Message: null);
        }
        catch (Exception ex)
        {
            return Error(ex);
        }
    }

    public async Task<AbdsServiceStatusDto> StartAsync(CancellationToken ct)
    {
        try
        {
            using var service = FindService();
            if (service is null)
                return NotInstalled();

            if (service.Status is ServiceControllerStatus.Running or ServiceControllerStatus.StartPending)
                return GetStatus();

            service.Start();
            await WaitForStatusAsync(service, ServiceControllerStatus.Running, TimeSpan.FromSeconds(20), ct);
            return GetStatus();
        }
        catch (Exception ex)
        {
            return Error(ex);
        }
    }

    public async Task<AbdsServiceStatusDto> RestartAsync(CancellationToken ct)
    {
        try
        {
            using var service = FindService();
            if (service is null)
                return NotInstalled();

            if (service.Status is ServiceControllerStatus.Running or ServiceControllerStatus.StartPending)
            {
                if (service.CanStop)
                {
                    service.Stop();
                    await WaitForStatusAsync(service, ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(20), ct);
                }
            }

            service.Refresh();
            if (service.Status is ServiceControllerStatus.Stopped or ServiceControllerStatus.StopPending)
            {
                service.Start();
                await WaitForStatusAsync(service, ServiceControllerStatus.Running, TimeSpan.FromSeconds(20), ct);
            }

            return GetStatus();
        }
        catch (Exception ex)
        {
            return Error(ex);
        }
    }

    private ServiceController? FindService()
    {
        return ServiceController.GetServices()
            .FirstOrDefault(s =>
                StringComparer.OrdinalIgnoreCase.Equals(s.ServiceName, serviceName) ||
                StringComparer.OrdinalIgnoreCase.Equals(s.DisplayName, serviceName));
    }

    private static Task WaitForStatusAsync(ServiceController service, ServiceControllerStatus status, TimeSpan timeout, CancellationToken ct)
    {
        return Task.Run(() =>
        {
            ct.ThrowIfCancellationRequested();
            service.WaitForStatus(status, timeout);
        }, ct);
    }

    private AbdsServiceStatusDto NotInstalled()
        => new(false, false, serviceName, serviceName, "NotInstalled", "ABDS Windows service is not installed.");

    private AbdsServiceStatusDto Error(Exception ex)
        => new(false, false, serviceName, serviceName, "Error", ex.Message);
}

public static class DestinationConfigService
{
    public static async Task<AbdsConfig> ProbeConfiguredDestinationsAsync(AbdsConfig config, CancellationToken ct)
    {
        foreach (var target in config.SyncPairs.SelectMany(p => p.TargetPaths).Concat(config.BackupSources.Select(b => b.BackupRootPath)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(target))
                continue;

            var result = await DestinationProbe.ProbeAsync(target, writeTest: true, ct);
            config = ApplyProbeResult(config, result);
        }

        return config;
    }

    public static AbdsConfig ApplyProbeResult(AbdsConfig config, DestinationProbeResult result)
    {
        var pairs = config.SyncPairs.Select(pair =>
        {
            var locations = new Dictionary<string, DestinationEndpoint>(pair.TargetLocations, StringComparer.OrdinalIgnoreCase);
            foreach (var target in pair.TargetPaths.Where(t => StringComparer.OrdinalIgnoreCase.Equals(t, result.Location)))
                locations[target] = EndpointFromResult(result);

            return pair with { TargetLocations = locations };
        }).ToList();

        var backups = config.BackupSources.Select(backup =>
            StringComparer.OrdinalIgnoreCase.Equals(backup.BackupRootPath, result.Location)
                ? backup with { BackupDestination = EndpointFromResult(result) }
                : backup).ToList();

        return config with { SyncPairs = pairs, BackupSources = backups };
    }

    private static DestinationEndpoint EndpointFromResult(DestinationProbeResult result)
        => new()
        {
            Location = result.Location,
            Kind = result.Kind,
            Identity = result.Identity,
            LastProbe = result
        };
}

public static class WindowsStartupControl
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "ABDS Tray Agent";

    public static StartupSettingDto GetStatus()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
            var value = key?.GetValue(ValueName) as string;
            return new StartupSettingDto(
                Enabled: value?.Contains("ABDS.TrayAgent.exe", StringComparison.OrdinalIgnoreCase) == true,
                RunValueName: ValueName,
                TrayAgentPath: ResolveTrayAgentPath(),
                Message: null);
        }
        catch (Exception ex)
        {
            return new StartupSettingDto(false, ValueName, ResolveTrayAgentPath(), ex.Message);
        }
    }

    public static StartupSettingDto SetEnabled(bool enabled)
    {
        try
        {
            var trayAgentPath = ResolveTrayAgentPath();
            using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath);

            if (enabled)
            {
                if (string.IsNullOrWhiteSpace(trayAgentPath))
                    return new StartupSettingDto(false, ValueName, null, "Nie znaleziono ABDS.TrayAgent.exe.");

                key.SetValue(ValueName, $"\"{trayAgentPath}\"");
            }
            else
            {
                key.DeleteValue(ValueName, throwOnMissingValue: false);
            }

            return GetStatus();
        }
        catch (Exception ex)
        {
            return new StartupSettingDto(false, ValueName, ResolveTrayAgentPath(), ex.Message);
        }
    }

    private static string? ResolveTrayAgentPath()
    {
        var published = Path.Combine(AppContext.BaseDirectory, "ABDS.TrayAgent.exe");
        if (File.Exists(published))
            return published;

        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "src", "ABDS.TrayAgent", "bin", "Debug", "net10.0-windows10.0.19041.0", "ABDS.TrayAgent.exe");
            if (File.Exists(candidate))
                return candidate;

            dir = dir.Parent;
        }

        return published;
    }
}
