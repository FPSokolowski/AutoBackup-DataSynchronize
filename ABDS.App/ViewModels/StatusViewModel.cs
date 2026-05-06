using System.Collections.ObjectModel;
using ABDS.App.Services;
using ABDS.SharedIpc;

namespace ABDS.App.ViewModels;

public partial class StatusViewModel : ObservableObject
{
    private readonly IpcService _ipc;

    public ObservableCollection<AbdsPairStatusDto> SyncPairs { get; } = new();
    public ObservableCollection<AbdsBackupStatusDto> BackupSources { get; } = new();

    [ObservableProperty] private string traySeverity = "-";
    [ObservableProperty] private string trayTooltip = "-";

    public StatusViewModel(IpcService ipc) => _ipc = ipc;

    [RelayCommand]
    public async Task RefreshAsync()
    {
        var snap = await _ipc.GetStatusAsync();
        if (snap is null)
            return;

        TraySeverity = snap.TraySeverity;
        TrayTooltip = snap.TrayTooltip;

        SyncPairs.Clear();
        foreach (var p in snap.SyncStatuses.OrderBy(x => x.SourcePath).ThenBy(x => x.TargetPath))
            SyncPairs.Add(p);

        BackupSources.Clear();
        foreach (var b in snap.BackupStatuses.OrderBy(x => x.SourcePath))
            BackupSources.Add(b);
    }

    [RelayCommand]
    public Task OpenPathAsync(string path)
    {
        try
        {
#if WINDOWS
            System.Diagnostics.Process.Start("explorer.exe", path);
#endif
        }
        catch { }
        return Task.CompletedTask;
    }
}