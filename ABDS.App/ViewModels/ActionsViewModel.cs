using ABDS.App.Services;
using ABDS.SharedIpc;

namespace ABDS.App.ViewModels;

public partial class ActionsViewModel : ObservableObject
{
    private readonly IpcService _ipc;
    private readonly TaskWindowService _taskWindow;

    [ObservableProperty] private string sourcePath = "";
    [ObservableProperty] private string targetPath = "";
    [ObservableProperty] private string backupRootPath = "";

    public ActionsViewModel(IpcService ipc, TaskWindowService taskWindow)
    {
        _ipc = ipc;
        _taskWindow = taskWindow;
    }

    [RelayCommand]
    public async Task SyncAllAsync()
    {
        var res = await _ipc.SendAsync(new AbdsCommand(AbdsCommandType.ForceSyncAll));
        if (res.Ok && res.RunId is not null)
            await _taskWindow.ShowRunAsync(res.RunId);
    }

    [RelayCommand]
    public async Task SyncPairAsync()
    {
        var res = await _ipc.SendAsync(new AbdsCommand(AbdsCommandType.ForceSyncPair, new()
        {
            ["sourcePath"] = SourcePath,
            ["targetPath"] = TargetPath
        }));
        if (res.Ok && res.RunId is not null)
            await _taskWindow.ShowRunAsync(res.RunId);
    }

    [RelayCommand]
    public async Task BackupAllAsync()
    {
        var res = await _ipc.SendAsync(new AbdsCommand(AbdsCommandType.ForceBackupAll));
        if (res.Ok && res.RunId is not null)
            await _taskWindow.ShowRunAsync(res.RunId);
    }

    [RelayCommand]
    public async Task BackupSourceAsync()
    {
        var res = await _ipc.SendAsync(new AbdsCommand(AbdsCommandType.ForceBackupSource, new()
        {
            ["sourcePath"] = SourcePath,
            ["backupRootPath"] = BackupRootPath
        }));
        if (res.Ok && res.RunId is not null)
            await _taskWindow.ShowRunAsync(res.RunId);
    }
}