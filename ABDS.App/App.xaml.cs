using ABDS.App.Services;

namespace ABDS.App;

public partial class App : Microsoft.Maui.Controls.Application
{
    public App(IServiceProvider sp)
    {
        InitializeComponent();
        MainPage = new AppShell();

        // parse args
        var args = Environment.GetCommandLineArgs();
        var idx = Array.FindIndex(args, a => a.Equals("--show-run", StringComparison.OrdinalIgnoreCase));
        if (idx >= 0 && idx + 1 < args.Length)
        {
            var runId = args[idx + 1];
            var svc = sp.GetRequiredService<TaskWindowService>();

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Shell.Current.GoToAsync("//ActionsPage");
                await svc.ShowRunAsync(runId);
            });
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}