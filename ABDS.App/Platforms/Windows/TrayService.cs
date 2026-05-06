
#if WINDOWS
using ABDS.App.Services;

namespace ABDS.App.Platforms.Windows;

public sealed class TrayService : IDisposable
{
    private readonly NotifyIcon _icon;
    private readonly IpcService _ipc;

    private readonly System.Windows.Forms.Timer _timer;

    public TrayService(IpcService ipc)
    {
        _ipc = ipc;

        _icon = new NotifyIcon
        {
            Visible = true,
            Text = "ABDS"
        };

        _icon.Icon = MakeIcon(Color.LimeGreen); // default OK
        _icon.Click += async (_, _) =>
        {
            // pokaż main window
            Application.Current?.OpenWindow(Application.Current.Windows[0]);
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                // na klik: przełącz na Status tab
                await Shell.Current.GoToAsync("//StatusPage");
            });
        };

        var menu = new ContextMenuStrip();
        menu.Items.Add("Open ABDS", null, async (_, _) =>
        {
            Application.Current?.OpenWindow(Application.Current.Windows[0]);
            await MainThread.InvokeOnMainThreadAsync(async () => await Shell.Current.GoToAsync("//StatusPage"));
        });
        menu.Items.Add("Exit", null, (_, _) =>
        {
            Dispose();
            Environment.Exit(0);
        });

        _icon.ContextMenuStrip = menu;

        _timer = new System.Windows.Forms.Timer { Interval = 10_000 }; // co 10s tooltip + kolor
        _timer.Tick += async (_, _) => await RefreshAsync();
        _timer.Start();
    }

    public async Task RefreshAsync()
    {
        var status = await _ipc.GetStatusAsync();
        if (status is null)
            return;

        var (color, text) = status.TraySeverity switch
        {
            "Busy" => (Color.DeepSkyBlue, "ABDS: wykonywanie zadania"),
            "Ok" => (Color.LimeGreen, "ABDS: OK"),
            "Warning" => (Color.Gold, "ABDS: WARNING"),
            "Critical" => (Color.Red, "ABDS: CRITICAL"),
            _ => (Color.Gray, "ABDS")
        };

        _icon.Icon = MakeIcon(color);

        // tooltip max ~63 znaki w NotifyIcon → skracamy
        var tooltip = status.TrayTooltip ?? text;
        tooltip = tooltip.Replace("\r", " ").Replace("\n", " ");
        if (tooltip.Length > 60)
            tooltip = tooltip[..60] + "...";

        _icon.Text = tooltip;
    }

    private static Icon MakeIcon(Color color)
    {
        // Prosty kolorowy kwadrat jako ikona. Docelowo: gotowe .ico per stan.
        var bmp = new Bitmap(16, 16);
        using var g = Graphics.FromImage(bmp);
        using var brush = new SolidBrush(color);
        g.FillEllipse(brush, 0, 0, 15, 15);

        return Icon.FromHandle(bmp.GetHicon());
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Dispose();

        _icon.Visible = false;
        _icon.Dispose();
    }
}
#endif