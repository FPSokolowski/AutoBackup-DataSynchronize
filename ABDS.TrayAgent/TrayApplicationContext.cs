using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text.Json;
using ABDS.SharedIpc;
using Microsoft.Win32;

namespace ABDS.TrayAgent;

public sealed class TrayApplicationContext : ApplicationContext
{
    private const string ServiceName = "ABDS (AutoBackup & DataSynchronize)";
    private const string DesktopHostPipeName = "ABDS_DESKTOP_HOST_PIPE_V1";
    private const string StartupValueName = "ABDS Tray Agent";
    internal const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

    private readonly NotifyIcon _notifyIcon;
    private readonly System.Windows.Forms.Timer _timer;
    private readonly AbdsIpcClient _ipc = new();
    private readonly HashSet<string> _knownFinishedRuns = new(StringComparer.OrdinalIgnoreCase);

    private bool _baselineLoaded;
    private string? _pendingNotificationRunId;
    private DateTimeOffset? _lastServiceProblemNotificationAt;

    public TrayApplicationContext()
    {
        _notifyIcon = new NotifyIcon
        {
            Text = "ABDS: sprawdzanie statusu...",
            Icon = TrayIconFactory.Create("Warning"),
            Visible = true,
            ContextMenuStrip = BuildMenu()
        };

        _notifyIcon.DoubleClick += (_, _) => OpenOrFocusUi();
        _notifyIcon.BalloonTipClicked += (_, _) => OpenOrFocusUi(_pendingNotificationRunId);

        _timer = new System.Windows.Forms.Timer { Interval = 5000 };
        _timer.Tick += async (_, _) => await RefreshAsync();
        _timer.Start();

        _ = RefreshAsync();
    }

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Otwórz ABDS", null, (_, _) => OpenOrFocusUi());
        menu.Items.Add("Status: sprawdzanie...");
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Uruchom usługę", null, async (_, _) => await StartServiceAsync());
        menu.Items.Add("Restartuj usługę", null, async (_, _) => await RestartServiceAsync());
        menu.Items.Add(new ToolStripSeparator());

        var startup = new ToolStripMenuItem("Startuj razem z Windows")
        {
            CheckOnClick = true,
            Checked = WindowsStartup.IsEnabled(StartupValueName)
        };
        startup.CheckedChanged += (_, _) => WindowsStartup.SetEnabled(StartupValueName, GetTrayAgentPath(), startup.Checked);
        menu.Items.Add(startup);

        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Zamknij ikonę tray", null, (_, _) => ExitThread());
        return menu;
    }

    private async Task RefreshAsync()
    {
        try
        {
            var response = await _ipc.SendAsync(new AbdsCommand(AbdsCommandType.GetStatus), CancellationToken.None);
            if (!response.Ok)
                throw new InvalidOperationException(response.Message);

            var status = JsonSerializer.Deserialize<AbdsStatusSnapshotDto>(response.Message, JsonOptions())!;
            ApplyStatus(status.TraySeverity, status.TrayTooltip);
            await NotifyForFinishedRunsAsync();
        }
        catch
        {
            var serviceStatus = GetServiceStatus();
            var severity = serviceStatus == ServiceControllerStatus.Running ? "Warning" : "Critical";
            ApplyStatus(severity, $"ABDS: brak połączenia gRPC. Usługa: {serviceStatus?.ToString() ?? "niezainstalowana"}");

            if (_lastServiceProblemNotificationAt is null ||
                DateTimeOffset.Now - _lastServiceProblemNotificationAt.Value > TimeSpan.FromMinutes(30))
            {
                _lastServiceProblemNotificationAt = DateTimeOffset.Now;
                ShowNotification(
                    "ABDS: problem z usługą",
                    "Nie udało się odczytać statusu usługi. Kliknij powiadomienie, aby otworzyć panel diagnostyki.",
                    ToolTipIcon.Error,
                    null);
            }
        }
    }

    private async Task NotifyForFinishedRunsAsync()
    {
        var response = await _ipc.SendAsync(new AbdsCommand(
            AbdsCommandType.GetRecentRuns,
            new Dictionary<string, string> { ["take"] = "20" }), CancellationToken.None);

        if (!response.Ok)
            return;

        var runs = JsonSerializer.Deserialize<List<AbdsRunDetailsDto>>(response.Message, JsonOptions()) ?? [];
        foreach (var run in runs.Where(r => r.FinishedAt is not null).OrderBy(r => r.FinishedAt))
        {
            if (!_baselineLoaded)
            {
                _knownFinishedRuns.Add(run.RunId);
                continue;
            }

            if (!_knownFinishedRuns.Add(run.RunId))
                continue;

            var shouldNotify =
                run.TaskType.Equals("Backup", StringComparison.OrdinalIgnoreCase) ||
                run.State is "Failed" or "PartiallyDone" or "Cancelled";

            if (!shouldNotify)
                continue;

            var icon = run.State == "Success" ? ToolTipIcon.Info :
                run.State == "PartiallyDone" ? ToolTipIcon.Warning :
                ToolTipIcon.Error;

            var title = run.TaskType.Equals("Backup", StringComparison.OrdinalIgnoreCase)
                ? $"ABDS: backup {TranslateState(run.State)}"
                : $"ABDS: zadanie {TranslateState(run.State)}";

            var source = run.Sources.FirstOrDefault() ?? "nieznane zrodlo";
            ShowNotification(title, $"{source}\nKliknij, aby otworzyć status i logi.", icon, run.RunId);
        }

        _baselineLoaded = true;
    }

    private void ApplyStatus(string severity, string tooltip)
    {
        var safeSeverity = string.IsNullOrWhiteSpace(severity) ? "Warning" : severity;
        var safeTooltip = string.IsNullOrWhiteSpace(tooltip) ? "ABDS" : tooltip;

        _notifyIcon.Icon?.Dispose();
        _notifyIcon.Icon = TrayIconFactory.Create(safeSeverity);
        _notifyIcon.Text = ShortTooltip(safeTooltip);

        if (_notifyIcon.ContextMenuStrip?.Items.Count > 1)
            _notifyIcon.ContextMenuStrip.Items[1].Text = $"Status: {TranslateSeverity(safeSeverity)}";
    }

    private async Task StartServiceAsync()
    {
        try
        {
            using var service = FindService();
            if (service is null)
            {
                ShowNotification("ABDS", "Usługa ABDS nie jest zainstalowana.", ToolTipIcon.Error, null);
                return;
            }

            if (service.Status is ServiceControllerStatus.Running or ServiceControllerStatus.StartPending)
                return;

            service.Start();
            await Task.Run(() => service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(20)));
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            ShowNotification("ABDS: nie udało się uruchomić usługi", ex.Message, ToolTipIcon.Error, null);
        }
    }

    private async Task RestartServiceAsync()
    {
        try
        {
            using var service = FindService();
            if (service is null)
            {
                ShowNotification("ABDS", "Usługa ABDS nie jest zainstalowana.", ToolTipIcon.Error, null);
                return;
            }

            if (service.Status == ServiceControllerStatus.Running && service.CanStop)
            {
                service.Stop();
                await Task.Run(() => service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(20)));
            }

            service.Refresh();
            if (service.Status == ServiceControllerStatus.Stopped)
            {
                service.Start();
                await Task.Run(() => service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(20)));
            }

            await RefreshAsync();
        }
        catch (Exception ex)
        {
            ShowNotification("ABDS: nie udało się zrestartować usługi", ex.Message, ToolTipIcon.Error, null);
        }
    }

    private static ServiceControllerStatus? GetServiceStatus()
    {
        try
        {
            using var service = FindService();
            return service?.Status;
        }
        catch
        {
            return null;
        }
    }

    private static ServiceController? FindService()
    {
        return ServiceController.GetServices()
            .FirstOrDefault(s =>
                StringComparer.OrdinalIgnoreCase.Equals(s.ServiceName, ServiceName) ||
                StringComparer.OrdinalIgnoreCase.Equals(s.DisplayName, ServiceName));
    }

    private void ShowNotification(string title, string message, ToolTipIcon icon, string? runId)
    {
        _pendingNotificationRunId = runId;
        _notifyIcon.BalloonTipTitle = title;
        _notifyIcon.BalloonTipText = message;
        _notifyIcon.BalloonTipIcon = icon;
        _notifyIcon.ShowBalloonTip(8000);
    }

    private static void OpenOrFocusUi(string? runId = null)
    {
        if (TrySendDesktopHostCommand(runId))
            return;

        var desktopHost = Path.Combine(AppContext.BaseDirectory, "ABDS.DesktopHost.exe");
        if (!File.Exists(desktopHost))
            desktopHost = FindDesktopHostInRepo() ?? desktopHost;

        Process.Start(new ProcessStartInfo
        {
            FileName = desktopHost,
            Arguments = string.IsNullOrWhiteSpace(runId) ? "" : $"--runId {runId}",
            UseShellExecute = true
        });
    }

    private static bool TrySendDesktopHostCommand(string? runId)
    {
        try
        {
            using var pipe = new NamedPipeClientStream(".", DesktopHostPipeName, PipeDirection.Out);
            pipe.Connect(250);
            using var writer = new StreamWriter(pipe) { AutoFlush = true };
            writer.WriteLine(string.IsNullOrWhiteSpace(runId) ? "show" : $"show|{runId}");
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string? FindDesktopHostInRepo()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "src", "ABDS.DesktopHost", "bin", "Debug", "net10.0-windows10.0.19041.0", "ABDS.DesktopHost.exe");
            if (File.Exists(candidate))
                return candidate;

            dir = dir.Parent;
        }

        return null;
    }

    private static string GetTrayAgentPath()
        => Path.Combine(AppContext.BaseDirectory, "ABDS.TrayAgent.exe");

    private static string ShortTooltip(string value)
    {
        var singleLine = value.Replace("\r", " ").Replace("\n", " ");
        return singleLine.Length <= 63 ? singleLine : singleLine[..60] + "...";
    }

    private static string TranslateSeverity(string severity)
        => severity switch
        {
            "Busy" => "w toku",
            "Ok" => "OK",
            "Warning" => "ostrzeżenie",
            "Critical" => "krytyczne",
            _ => severity
        };

    private static string TranslateState(string state)
        => state switch
        {
            "Success" => "zakończony",
            "Failed" => "z błędem",
            "PartiallyDone" => "częściowy",
            "Cancelled" => "anulowany",
            _ => state.ToLowerInvariant()
        };

    private static JsonSerializerOptions JsonOptions()
        => new() { PropertyNameCaseInsensitive = true };

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer.Dispose();
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }

        base.Dispose(disposing);
    }
}

internal static class TrayIconFactory
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    public static Icon Create(string severity)
    {
        using var baseImage = LoadBaseImage();
        using var bitmap = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);
            g.DrawImage(baseImage, new Rectangle(0, 0, 32, 32));

            using var brush = new SolidBrush(ColorFor(severity));
            using var border = new Pen(Color.White, 2);
            var badge = new Rectangle(20, 20, 10, 10);
            g.FillEllipse(brush, badge);
            g.DrawEllipse(border, badge);
        }

        var handle = bitmap.GetHicon();
        try
        {
            using var icon = Icon.FromHandle(handle);
            return (Icon)icon.Clone();
        }
        finally
        {
            DestroyIcon(handle);
        }
    }

    private static Image LoadBaseImage()
    {
        var png = Path.Combine(AppContext.BaseDirectory, "Resources", "icon.png");
        if (File.Exists(png))
            return Image.FromFile(png);

        return Icon.ExtractAssociatedIcon(Application.ExecutablePath)?.ToBitmap()
            ?? SystemIcons.Application.ToBitmap();
    }

    private static Color ColorFor(string severity)
        => severity switch
        {
            "Busy" => Color.FromArgb(59, 130, 246),
            "Ok" => Color.FromArgb(34, 197, 94),
            "Warning" => Color.FromArgb(245, 158, 11),
            "Critical" => Color.FromArgb(239, 68, 68),
            _ => Color.FromArgb(245, 158, 11)
        };
}

internal static class WindowsStartup
{
    public static bool IsEnabled(string valueName)
    {
        using var key = Registry.CurrentUser.OpenSubKey(TrayApplicationContext.RunKeyPath, writable: false);
        return key?.GetValue(valueName) is string value &&
            value.Contains("ABDS.TrayAgent.exe", StringComparison.OrdinalIgnoreCase);
    }

    public static void SetEnabled(string valueName, string exePath, bool enabled)
    {
        using var key = Registry.CurrentUser.CreateSubKey(TrayApplicationContext.RunKeyPath);
        if (enabled)
            key.SetValue(valueName, $"\"{exePath}\"");
        else
            key.DeleteValue(valueName, throwOnMissingValue: false);
    }
}
