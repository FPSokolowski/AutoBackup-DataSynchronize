using ABDS.App.Services;
using ABDS.App.ViewModels;
using Microsoft.Extensions.Logging;

#if WINDOWS
using ABDS.App.Platforms.Windows;
#endif

namespace ABDS.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // services
        builder.Services.AddSingleton<IpcService>();
        builder.Services.AddSingleton<TaskWindowService>();

        // VMs
        builder.Services.AddTransient<StatusViewModel>();
        builder.Services.AddTransient<ActionsViewModel>();
        builder.Services.AddTransient<TaskWindowViewModel>();

        // Views
        builder.Services.AddTransient<Views.StatusPage>();
        builder.Services.AddTransient<Views.ActionsPage>();
        builder.Services.AddTransient<Views.ConfigPage>(); // placeholder niżej
        builder.Services.AddTransient<Views.TaskWindowPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

#if WINDOWS
        builder.Services.AddSingleton<TrayService>();
#endif

        var app = builder.Build();

#if WINDOWS
        // init tray
        app.Services.GetRequiredService<TrayService>();
#endif

        return app;
    }

}
