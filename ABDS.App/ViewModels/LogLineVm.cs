using CommunityToolkit.Mvvm.ComponentModel;

namespace ABDS.App.ViewModels;

public sealed class LogLineVm : ObservableObject
{
    public DateTimeOffset At { get; }
    public string Level { get; }
    public string Message { get; }

    public Color Foreground => Level switch
    {
        "INFO" => Colors.LightGray,
        "WARN" => Colors.Gold,
        "ERROR" => Colors.OrangeRed,
        "SUCCESS" => Colors.LimeGreen,
        _ => Colors.White
    };

    public LogLineVm(DateTimeOffset at, string level, string message)
    {
        At = at;
        Level = level;
        Message = message;
    }

    public string Text => $"{At:HH:mm:ss} [{Level}] {Message}";
}