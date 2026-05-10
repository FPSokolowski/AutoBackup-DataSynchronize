using System.Diagnostics;
using System.IO.Pipes;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text.Json;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace ABDS.DesktopHost;

public sealed class MainForm : Form
{
    private const string ServiceName = "ABDS (AutoBackup & DataSynchronize)";
    private const string WebUrl = "http://localhost:5076";
    private readonly string _pipeName;
    private string? _runId;

    private readonly WebView2 _webView = new() { Dock = DockStyle.Fill };
    private readonly Label _statusLabel = new()
    {
        Dock = DockStyle.Top,
        Height = 32,
        TextAlign = ContentAlignment.MiddleLeft,
        Padding = new Padding(12, 0, 12, 0)
    };

    private Process? _webHostProcess;
    private CancellationTokenSource? _pipeCts;
    private bool _webViewReady;
    private bool _darkWindowTheme = true;

    public MainForm(string pipeName, string? runId)
    {
        _pipeName = pipeName;
        _runId = runId;
        Text = "ABDS";
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        Width = 1280;
        Height = 840;
        MinimumSize = new Size(1024, 700);
        StartPosition = FormStartPosition.CenterScreen;
        RestoreWindowPlacement();

        Controls.Add(_webView);
        Controls.Add(_statusLabel);

        Shown += async (_, _) => await InitializeAsync();
        Move += (_, _) => SaveWindowPlacement();
        ResizeEnd += (_, _) => SaveWindowPlacement();
        FormClosing += (_, _) => SaveWindowPlacement();
        FormClosed += (_, _) =>
        {
            _pipeCts?.Cancel();
            StopOwnedWebHost();
        };
    }

    private async Task InitializeAsync()
    {
        try
        {
            SetStatus("Sprawdzam usługę ABDS...");
            var serviceResult = await TryStartServiceAsync();

            SetStatus(serviceResult);
            await Task.Delay(400);

            SetStatus("Uruchamiam lokalny panel ABDS...");
            await EnsureWebHostAsync();

            SetStatus("Ładowanie UI...");
            var webViewEnvironment = await CreateWebViewEnvironmentAsync();
            await _webView.EnsureCoreWebView2Async(webViewEnvironment);
            await ClearWebViewCacheAfterVersionChangeAsync(_webView.CoreWebView2);
            ConfigureWebView(_webView.CoreWebView2);
            ConfigureWebViewMessages(_webView.CoreWebView2);
            _webView.Source = new Uri(BuildStartUrl());
            _webViewReady = true;
            StartCommandPipe();

            SetStatus("");
            _statusLabel.Visible = false;
        }
        catch (Exception ex)
        {
            ShowStartupError(ex);
        }
    }

    private static async Task<CoreWebView2Environment> CreateWebViewEnvironmentAsync()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(localAppData))
            localAppData = Path.GetTempPath();

        var userDataFolder = Path.Combine(localAppData, "ABDS", "WebView2");
        Directory.CreateDirectory(userDataFolder);

        return await CoreWebView2Environment.CreateAsync(
            browserExecutableFolder: null,
            userDataFolder: userDataFolder);
    }

    private static void ConfigureWebView(CoreWebView2 core)
    {
        core.Settings.AreDevToolsEnabled = false;
        core.Settings.AreDefaultContextMenusEnabled = true;
        core.Settings.AreBrowserAcceleratorKeysEnabled = false;
        core.Settings.IsStatusBarEnabled = false;
        core.Settings.IsZoomControlEnabled = true;
    }

    private void ConfigureWebViewMessages(CoreWebView2 core)
    {
        core.WebMessageReceived += (_, args) =>
        {
            try
            {
                using var payload = JsonDocument.Parse(args.WebMessageAsJson);
                var root = payload.RootElement;
                if (!root.TryGetProperty("type", out var type))
                    return;

                switch (type.GetString())
                {
                    case "abds-theme":
                        if (root.TryGetProperty("dark", out var dark) && dark.ValueKind is JsonValueKind.True or JsonValueKind.False)
                            ApplyWindowTheme(dark.GetBoolean());
                        break;
                    case "abds-browse-folder":
                        HandleBrowseFolderMessage(core, root);
                        break;
                }
            }
            catch
            {
                // WebView messages are convenience features; ignore malformed payloads.
            }
        };
    }

    private void HandleBrowseFolderMessage(CoreWebView2 core, JsonElement root)
    {
        if (!root.TryGetProperty("requestId", out var requestIdElement))
            return;

        var requestId = requestIdElement.GetString();
        if (string.IsNullOrWhiteSpace(requestId))
            return;

        var currentPath = root.TryGetProperty("currentPath", out var currentPathElement)
            ? currentPathElement.GetString()
            : null;

        using var dialog = new FolderBrowserDialog
        {
            Description = "Wskaż folder dla ABDS",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true
        };

        if (!string.IsNullOrWhiteSpace(currentPath) && Directory.Exists(currentPath))
            dialog.SelectedPath = currentPath;

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        var message = JsonSerializer.Serialize(new
        {
            type = "abds-folder-selected",
            requestId,
            path = dialog.SelectedPath
        });
        core.PostWebMessageAsJson(message);
    }

    private string BuildStartUrl()
    {
        return string.IsNullOrWhiteSpace(_runId)
            ? WebUrl
            : $"{WebUrl}/?runId={Uri.EscapeDataString(_runId)}";
    }

    private static async Task ClearWebViewCacheAfterVersionChangeAsync(CoreWebView2 core)
    {
        var currentVersion = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion
            ?? Application.ProductVersion
            ?? "0.0.0";
        var markerPath = GetWebViewCacheVersionPath();
        var previousVersion = File.Exists(markerPath)
            ? await File.ReadAllTextAsync(markerPath)
            : null;

        if (string.Equals(previousVersion?.Trim(), currentVersion, StringComparison.OrdinalIgnoreCase))
            return;

        await core.Profile.ClearBrowsingDataAsync(CoreWebView2BrowsingDataKinds.DiskCache);
        Directory.CreateDirectory(Path.GetDirectoryName(markerPath)!);
        await File.WriteAllTextAsync(markerPath, currentVersion);
    }

    public static string? ReadRunId(string[] args)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (args[i].Equals("--runId", StringComparison.OrdinalIgnoreCase))
                return args[i + 1];
        }

        return null;
    }

    private void StartCommandPipe()
    {
        _pipeCts = new CancellationTokenSource();
        _ = Task.Run(() => ListenForCommandsAsync(_pipeCts.Token));
    }

    private async Task ListenForCommandsAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var pipe = new NamedPipeServerStream(
                    _pipeName,
                    PipeDirection.In,
                    maxNumberOfServerInstances: 1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                await pipe.WaitForConnectionAsync(ct);
                using var reader = new StreamReader(pipe);
                var command = await reader.ReadLineAsync(ct);
                BeginInvoke(() => HandleExternalCommand(command));
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch
            {
                await Task.Delay(500, ct);
            }
        }
    }

    private void HandleExternalCommand(string? command)
    {
        var runId = ParseRunIdFromCommand(command);
        BringWindowToFront();
        if (!string.IsNullOrWhiteSpace(runId))
            NavigateToRun(runId);
    }

    private static string? ParseRunIdFromCommand(string? command)
    {
        if (string.IsNullOrWhiteSpace(command))
            return null;

        var parts = command.Split('|', 2);
        return parts.Length == 2 && parts[0].Equals("show", StringComparison.OrdinalIgnoreCase)
            ? parts[1]
            : null;
    }

    private void NavigateToRun(string runId)
    {
        _runId = runId;
        if (!_webViewReady)
            return;

        _webView.Source = new Uri(BuildStartUrl());
    }

    private void BringWindowToFront()
    {
        if (WindowState == FormWindowState.Minimized)
            WindowState = FormWindowState.Normal;

        Show();
        Activate();
        TopMost = true;
        TopMost = false;
        SetForegroundWindow(Handle);
    }

    private async Task<string> TryStartServiceAsync()
    {
        try
        {
            using var service = FindService();
            if (service is null)
                return "Usługa ABDS nie jest zainstalowana. Panel pokaże status offline.";

            if (service.Status == ServiceControllerStatus.Running)
                return "Usługa ABDS działa.";

            if (service.Status is ServiceControllerStatus.StartPending)
                return "Usługa ABDS jest uruchamiana.";

            if (service.Status == ServiceControllerStatus.Stopped)
            {
                service.Start();
                await Task.Run(() => service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(20)));
                return "Usługa ABDS została uruchomiona.";
            }

            return $"Usługa ABDS ma stan: {service.Status}.";
        }
        catch (Exception ex)
        {
            return $"Nie udało się uruchomić usługi ABDS: {ex.Message}";
        }
    }

    private static ServiceController? FindService()
    {
        return ServiceController.GetServices()
            .FirstOrDefault(s =>
                StringComparer.OrdinalIgnoreCase.Equals(s.ServiceName, ServiceName) ||
                StringComparer.OrdinalIgnoreCase.Equals(s.DisplayName, ServiceName));
    }

    private async Task EnsureWebHostAsync()
    {
        if (await IsWebHostAliveAsync())
            return;

        var process = CreateWebHostProcess();
        process.Start();
        _webHostProcess = process;

        var deadline = DateTimeOffset.Now + TimeSpan.FromSeconds(25);
        while (DateTimeOffset.Now < deadline)
        {
            if (process.HasExited)
                throw new InvalidOperationException("Lokalny panel ABDS zakończył działanie podczas startu.");

            if (await IsWebHostAliveAsync())
                return;

            await Task.Delay(500);
        }

        throw new System.TimeoutException("Lokalny panel ABDS nie odpowiedział w wyznaczonym czasie.");
    }

    private static async Task<bool> IsWebHostAliveAsync()
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            using var response = await client.GetAsync(WebUrl);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static Process CreateWebHostProcess()
    {
        var baseDir = AppContext.BaseDirectory;
        var webExe = Path.Combine(baseDir, "ABDS.Web.exe");
        if (File.Exists(webExe))
        {
            return new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = webExe,
                    Arguments = $"--urls {WebUrl}",
                    WorkingDirectory = baseDir,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };
        }

        var project = FindWebProjectFile();
        if (project is null)
            throw new FileNotFoundException("Nie znaleziono ABDS.Web.exe ani projektu ABDS.Web.csproj.");

        return new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{project}\" --no-restore --urls {WebUrl}",
                WorkingDirectory = Path.GetDirectoryName(project)!,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }
        };
    }

    private static string? FindWebProjectFile()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "src", "ABDS.Web", "ABDS.Web.csproj");
            if (File.Exists(candidate))
                return candidate;

            dir = dir.Parent;
        }

        return null;
    }

    private void ShowStartupError(Exception ex)
    {
        SetStatus("Nie udało się uruchomić panelu ABDS.");
        _webView.Visible = false;

        var text = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            ScrollBars = ScrollBars.Vertical,
            Text = "Nie udało się uruchomić ABDS Desktop Host." + Environment.NewLine + Environment.NewLine + ex
        };
        Controls.Add(text);
        text.BringToFront();
    }

    private void StopOwnedWebHost()
    {
        try
        {
            if (_webHostProcess is { HasExited: false })
                _webHostProcess.Kill(entireProcessTree: true);
        }
        catch
        {
            // Closing the UI should never throw.
        }
    }

    private void SetStatus(string message)
    {
        _statusLabel.Visible = !string.IsNullOrWhiteSpace(message);
        _statusLabel.Text = message;
    }

    private void ApplyWindowTheme(bool dark)
    {
        _darkWindowTheme = dark;
        if (!IsHandleCreated)
            return;

        try
        {
            var darkMode = dark ? 1 : 0;
            DwmSetWindowAttribute(Handle, DwmwaUseImmersiveDarkMode, ref darkMode, sizeof(int));

            var caption = dark ? Rgb(17, 24, 39) : Rgb(245, 247, 251);
            var text = dark ? Rgb(241, 245, 249) : Rgb(15, 23, 42);
            DwmSetWindowAttribute(Handle, DwmwaCaptionColor, ref caption, sizeof(int));
            DwmSetWindowAttribute(Handle, DwmwaTextColor, ref text, sizeof(int));
        }
        catch
        {
            // Older Windows builds may not support these DWM attributes.
        }
    }

    private static int Rgb(byte red, byte green, byte blue)
    {
        return red | ( green << 8 ) | ( blue << 16 );
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        ApplyWindowTheme(_darkWindowTheme);
    }

    private void RestoreWindowPlacement()
    {
        var placement = LoadWindowPlacement();
        if (placement is null)
            return;

        var bounds = new Rectangle(placement.Left, placement.Top, placement.Width, placement.Height);
        if (bounds.Width < MinimumSize.Width || bounds.Height < MinimumSize.Height || !IsVisibleOnAnyScreen(bounds))
            return;

        StartPosition = FormStartPosition.Manual;
        Bounds = bounds;

        if (placement.WindowState is FormWindowState.Maximized or FormWindowState.Normal)
            WindowState = placement.WindowState;
    }

    private void SaveWindowPlacement()
    {
        try
        {
            if (WindowState == FormWindowState.Minimized)
                return;

            var bounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
            if (bounds.Width < MinimumSize.Width || bounds.Height < MinimumSize.Height)
                return;

            var placement = new WindowPlacement(
                bounds.Left,
                bounds.Top,
                bounds.Width,
                bounds.Height,
                WindowState);

            var path = GetWindowPlacementPath();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(placement, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch
        {
            // Window placement is a convenience setting; startup and shutdown should not depend on it.
        }
    }

    private static WindowPlacement? LoadWindowPlacement()
    {
        try
        {
            var path = GetWindowPlacementPath();
            return File.Exists(path)
                ? JsonSerializer.Deserialize<WindowPlacement>(File.ReadAllText(path))
                : null;
        }
        catch
        {
            return null;
        }
    }

    private static string GetWindowPlacementPath()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(localAppData))
            localAppData = Path.GetTempPath();

        return Path.Combine(localAppData, "ABDS", "window-state.json");
    }

    private static string GetWebViewCacheVersionPath()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(localAppData))
            localAppData = Path.GetTempPath();

        return Path.Combine(localAppData, "ABDS", "webview-cache-version.txt");
    }

    private static bool IsVisibleOnAnyScreen(Rectangle bounds)
    {
        return Screen.AllScreens.Any(screen => Rectangle.Intersect(screen.WorkingArea, bounds).Width >= 120
            && Rectangle.Intersect(screen.WorkingArea, bounds).Height >= 80);
    }

    private sealed record WindowPlacement(int Left, int Top, int Width, int Height, FormWindowState WindowState);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    private const int DwmwaUseImmersiveDarkMode = 20;
    private const int DwmwaCaptionColor = 35;
    private const int DwmwaTextColor = 36;

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int attributeValue, int attributeSize);
}
