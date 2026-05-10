using System.Text.Json;
using ABDS.Core.Destinations;
using ABDS.Core.Models;
using ABDS.SharedIpc;
using System.ServiceProcess;
using Microsoft.Win32;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<AbdsIpcClient>();
builder.Services.AddSingleton(AbdsWebPaths.Default());
builder.Services.AddSingleton(new AbdsServiceControl("ABDS (AutoBackup & DataSynchronize)"));
builder.Services.AddHttpClient();

var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    WriteIndented = true
};

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/config", async (AbdsWebPaths paths, CancellationToken ct) =>
{
    var config = await LoadConfigAsync(paths, jsonOptions, ct);
    return Results.Json(config, jsonOptions);
});

app.MapPut("/api/config", async (AbdsConfig config, AbdsWebPaths paths, CancellationToken ct) =>
{
    config = NormalizeBackupNames(config);
    var validationError = ValidateConfig(config);
    if (validationError is not null)
        return Results.BadRequest(new { message = validationError });

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
    var config = await LoadConfigAsync(paths, jsonOptions, ct);
    var result = await DestinationProbe.ProbeAsync(request.Location, writeTest: true, ct);
    config = DestinationConfigService.ApplyProbeResult(config, result);
    await SaveConfigAsync(config, paths, jsonOptions, ct);
    return Results.Json(new DestinationRetestResponse(result, config), jsonOptions);
});

app.MapPost("/api/destinations/retest-sync", async (SyncDestinationRetestRequest request, AbdsWebPaths paths, CancellationToken ct) =>
{
    var config = await LoadConfigAsync(paths, jsonOptions, ct);
    var sourceResult = await DestinationProbe.ProbeAsync(request.SourcePath, writeTest: false, ct);
    var targetResult = await DestinationProbe.ProbeAsync(request.TargetPath, writeTest: true, ct);
    config = DestinationConfigService.ApplyProbeResult(config, sourceResult);
    config = DestinationConfigService.ApplyProbeResult(config, targetResult);
    await SaveConfigAsync(config, paths, jsonOptions, ct);
    return Results.Json(new SyncDestinationRetestResponse(sourceResult, targetResult, config), jsonOptions);
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

app.MapGet("/api/update/current", () =>
    Results.Json(UpdateService.Current(), jsonOptions));

app.MapGet("/api/update/check", async (AbdsWebPaths paths, IHttpClientFactory httpFactory, CancellationToken ct) =>
{
    var config = await LoadConfigAsync(paths, jsonOptions, ct);
    var result = await UpdateService.CheckAsync(ResolveUpdateManifestUrl(config), httpFactory.CreateClient(), ct);
    return Results.Json(result, jsonOptions);
});

app.MapPost("/api/update/install", async (AbdsWebPaths paths, IHttpClientFactory httpFactory, CancellationToken ct) =>
{
    var config = await LoadConfigAsync(paths, jsonOptions, ct);
    var result = await UpdateService.DownloadAndLaunchAsync(ResolveUpdateManifestUrl(config), httpFactory.CreateClient(), ct);
    return Results.Json(result, jsonOptions);
});

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

static string? ValidateConfig(AbdsConfig config)
{
    var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    foreach (var backup in config.BackupSources)
    {
        if (string.IsNullOrWhiteSpace(backup.Name))
            return "Backup source name is required.";

        if (!names.Add(backup.Name.Trim()))
            return $"Backup source name must be unique: {backup.Name}";
    }

    return null;
}

static AbdsConfig NormalizeBackupNames(AbdsConfig config)
{
    var used = config.BackupSources
        .Select(backup => backup.Name?.Trim())
        .Where(name => !string.IsNullOrWhiteSpace(name))
        .ToHashSet(StringComparer.OrdinalIgnoreCase);
    var backups = config.BackupSources.Select((backup, index) =>
    {
        if (!string.IsNullOrWhiteSpace(backup.Name))
            return backup with { Name = backup.Name.Trim() };

        var name = !string.IsNullOrWhiteSpace(backup.SourcePath)
            ? Path.GetFileName(backup.SourcePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
            : $"Backup {index + 1}";
        name = string.IsNullOrWhiteSpace(name) ? $"Backup {index + 1}" : name.Trim();
        var candidate = name;
        var suffix = 2;
        while (!used.Add(candidate))
            candidate = $"{name} {suffix++}";

        return backup with { Name = candidate };
    }).ToList();

    return config with { BackupSources = backups };
}

static string ResolveUpdateManifestUrl(AbdsConfig config)
    => string.IsNullOrWhiteSpace(config.Update.ManifestUrl)
        ? new AbdsUpdateConfig().ManifestUrl
        : config.Update.ManifestUrl;

static async Task<AbdsConfig> LoadConfigAsync(AbdsWebPaths paths, JsonSerializerOptions options, CancellationToken ct)
{
    Directory.CreateDirectory(paths.RootDir);

    if (!File.Exists(paths.ConfigPath))
    {
        var config = new AbdsConfig();
        await SaveConfigAsync(config, paths, new JsonSerializerOptions { WriteIndented = true }, ct);
        return config;
    }

    var content = await File.ReadAllTextAsync(paths.ConfigPath, ct);
    return JsonSerializer.Deserialize<AbdsConfig>(content, options) ?? new AbdsConfig();
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
public sealed record SyncDestinationRetestRequest(string SourcePath, string TargetPath);
public sealed record SyncDestinationRetestResponse(DestinationProbeResult SourceResult, DestinationProbeResult TargetResult, AbdsConfig Config);
public sealed record StartupSettingRequest(bool Enabled);
public sealed record StartupSettingDto(bool Enabled, string RunValueName, string? TrayAgentPath, string? Message);
public sealed record UpdateManifest(string Version, string? ReleaseNotes, List<UpdateInstallerAsset> Installers);
public sealed record UpdateInstallerAsset(string Runtime, string Url, string? Sha256, long? SizeBytes);
public sealed record UpdateCheckDto(
    string CurrentVersion,
    string? LatestVersion,
    string Runtime,
    bool UpdateAvailable,
    string? InstallerUrl,
    string? ReleaseNotes,
    string? Message);

public sealed record UpdateInstallDto(bool Started, string? InstallerPath, string Message);

public sealed record AbdsServiceStatusDto(
    bool Installed,
    bool CanControl,
    string ServiceName,
    string DisplayName,
    string Status,
    string? Message);

public static class UpdateService
{
    public static UpdateCheckDto Current()
        => new(
            CurrentVersion(),
            null,
            CurrentRuntime(),
            false,
            null,
            null,
            "Version check has not been run yet.");

    public static async Task<UpdateCheckDto> CheckAsync(string? manifestUrl, HttpClient http, CancellationToken ct)
    {
        var currentVersion = CurrentVersion();
        var runtime = CurrentRuntime();
        if (string.IsNullOrWhiteSpace(manifestUrl))
            return new UpdateCheckDto(currentVersion, null, runtime, false, null, null, "Update manifest URL is not configured.");

        UpdateManifest manifest;
        try
        {
            manifest = await FetchManifestAsync(manifestUrl, http, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or InvalidOperationException)
        {
            return new UpdateCheckDto(
                currentVersion,
                null,
                runtime,
                false,
                null,
                null,
                $"Could not read update manifest: {ex.Message}");
        }

        var asset = SelectAsset(manifest, runtime);
        if (asset is null)
            return new UpdateCheckDto(currentVersion, manifest.Version, runtime, false, null, manifest.ReleaseNotes, $"No installer for {runtime}.");

        var updateAvailable = CompareVersions(manifest.Version, currentVersion) > 0;
        return new UpdateCheckDto(
            currentVersion,
            manifest.Version,
            runtime,
            updateAvailable,
            updateAvailable ? asset.Url : null,
            manifest.ReleaseNotes,
            updateAvailable ? "Update available." : "ABDS is up to date.");
    }

    public static async Task<UpdateInstallDto> DownloadAndLaunchAsync(string? manifestUrl, HttpClient http, CancellationToken ct)
    {
        var check = await CheckAsync(manifestUrl, http, ct);
        if (!check.UpdateAvailable || string.IsNullOrWhiteSpace(check.InstallerUrl))
            return new UpdateInstallDto(false, null, check.Message ?? "No update available.");

        var manifest = await FetchManifestAsync(manifestUrl!, http, ct);
        var asset = SelectAsset(manifest, check.Runtime)
            ?? throw new InvalidOperationException($"No installer for {check.Runtime}.");
        var updatesDir = Path.Combine(Path.GetTempPath(), "ABDS", "Updates");
        Directory.CreateDirectory(updatesDir);
        var fileName = Path.GetFileName(new Uri(asset.Url).LocalPath);
        if (string.IsNullOrWhiteSpace(fileName))
            fileName = $"ABDS-Setup-{manifest.Version}-{check.Runtime}.exe";
        var installerPath = Path.Combine(updatesDir, fileName);

        await using (var source = await http.GetStreamAsync(asset.Url, ct))
        await using (var target = File.Create(installerPath))
        {
            await source.CopyToAsync(target, ct);
        }

        if (!string.IsNullOrWhiteSpace(asset.Sha256))
            await VerifySha256Async(installerPath, asset.Sha256, ct);

        Process.Start(new ProcessStartInfo
        {
            FileName = installerPath,
            UseShellExecute = true,
            Verb = "open"
        });

        return new UpdateInstallDto(true, installerPath, "Installer downloaded and started.");
    }

    private static async Task<UpdateManifest> FetchManifestAsync(string manifestUrl, HttpClient http, CancellationToken ct)
        => await http.GetFromJsonAsync<UpdateManifest>(manifestUrl, cancellationToken: ct)
           ?? throw new InvalidOperationException("Update manifest is empty or invalid.");

    private static UpdateInstallerAsset? SelectAsset(UpdateManifest manifest, string runtime)
        => manifest.Installers.FirstOrDefault(asset => StringComparer.OrdinalIgnoreCase.Equals(asset.Runtime, runtime));

    private static string CurrentVersion()
        => Assembly.GetEntryAssembly()?
               .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
               .InformationalVersion
           ?? "0.0.0";

    private static string CurrentRuntime()
        => RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X86 => "win-x86",
            Architecture.Arm64 => "win-arm64",
            _ => "win-x64"
        };

    private static int CompareVersions(string left, string right)
    {
        var leftVersion = ToVersion(left);
        var rightVersion = ToVersion(right);
        return leftVersion.CompareTo(rightVersion);
    }

    private static Version ToVersion(string value)
    {
        var core = value.Split('-', '+')[0];
        var parts = core.Split('.')
            .Select(part => int.TryParse(part, out var number) ? number : 0)
            .ToList();
        while (parts.Count < 4)
            parts.Add(0);

        return new Version(parts[0], parts[1], parts[2], parts[3]);
    }

    private static async Task VerifySha256Async(string filePath, string expectedSha256, CancellationToken ct)
    {
        await using var file = File.OpenRead(filePath);
        var hash = await SHA256.HashDataAsync(file, ct);
        var actual = Convert.ToHexString(hash).ToLowerInvariant();
        if (!StringComparer.OrdinalIgnoreCase.Equals(actual, expectedSha256.Trim()))
            throw new InvalidOperationException("Downloaded installer checksum does not match the update manifest.");
    }
}

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

            var source = StringComparer.OrdinalIgnoreCase.Equals(pair.SourcePath, result.Location)
                ? EndpointFromResult(result)
                : pair.SourceLocation;

            return pair with { SourceLocation = source, TargetLocations = locations };
        }).ToList();

        var backups = config.BackupSources.Select(backup =>
            StringComparer.OrdinalIgnoreCase.Equals(backup.BackupRootPath, result.Location)
                ? backup with { BackupDestination = EndpointFromResult(result) }
                : StringComparer.OrdinalIgnoreCase.Equals(backup.SourcePath, result.Location)
                    ? backup with { SourceLocation = EndpointFromResult(result) }
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
