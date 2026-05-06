using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.ServiceProcess;
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

        Controls.Add(_webView);
        Controls.Add(_statusLabel);

        Shown += async (_, _) => await InitializeAsync();
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
            await _webView.EnsureCoreWebView2Async();
            ConfigureWebView(_webView.CoreWebView2);
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

    private static void ConfigureWebView(CoreWebView2 core)
    {
        core.Settings.AreDevToolsEnabled = false;
        core.Settings.AreDefaultContextMenusEnabled = true;
        core.Settings.AreBrowserAcceleratorKeysEnabled = false;
        core.Settings.IsStatusBarEnabled = false;
        core.Settings.IsZoomControlEnabled = true;
    }

    private string BuildStartUrl()
    {
        return string.IsNullOrWhiteSpace(_runId)
            ? WebUrl
            : $"{WebUrl}/?runId={Uri.EscapeDataString(_runId)}";
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

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
}
