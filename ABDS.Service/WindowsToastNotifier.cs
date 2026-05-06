#if WINDOWS
using CommunityToolkit.WinUI.Notifications;

namespace ABDS.Service;

public static class WindowsToastNotifier
{
    public static void ShowCritical(string title, string message)
    {
        try
        {
            new ToastContentBuilder()
                .AddText(title)
                .AddText(message)
                .Show();
        }
        catch
        {
            // best-effort
        }
    }
}
#endif