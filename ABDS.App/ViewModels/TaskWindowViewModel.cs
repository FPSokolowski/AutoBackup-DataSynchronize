using System.Collections.ObjectModel;
using ABDS.App.Services;
using ABDS.SharedIpc;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ABDS.App.ViewModels;

public partial class TaskWindowViewModel : ObservableObject
{
    private readonly IpcService _ipc;
    private CancellationTokenSource? _loopCts;

    public ObservableCollection<LogLineVm> Logs { get; } = new();

    [ObservableProperty] private string runId = "";
    [ObservableProperty] private string taskType = "";
    [ObservableProperty] private string state = "";
    [ObservableProperty] private string summary = "";
    [ObservableProperty] private int trackedPathsCount;

    [ObservableProperty] private bool isCancelEnabled;
    [ObservableProperty] private bool isOkEnabled;

    [ObservableProperty] private double progressValue; // 0..1
    [ObservableProperty] private string progressText = "0%";

    [ObservableProperty] private string bytesText = "0 / 0";

    public List<string> Sources { get; private set; } = new();
    public List<string> Targets { get; private set; } = new();

    private int _lastLogCount = 0;

    public TaskWindowViewModel(IpcService ipc) => _ipc = ipc;

    public async Task StartAsync(string runId)
    {
        RunId = runId;
        _loopCts = new CancellationTokenSource();

        await LoopAsync(_loopCts.Token);
    }

    private async Task LoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var details = await _ipc.GetRunDetailsAsync(RunId, ct);
            if (details is not null)
            {
                TaskType = details.TaskType;
                State = details.State;
                Summary = details.Summary ?? "";
                Sources = details.Sources;
                Targets = details.Targets;
                TrackedPathsCount = ( details.Sources.Count + details.Targets.Count );

                var total = Math.Max(1, details.TotalBytes);
                var copied = Math.Max(0, details.CopiedBytes);
                ProgressValue = Math.Clamp((double)copied / total, 0, 1);
                ProgressText = $"{(int)( ProgressValue * 100 )}%";
                bytesText = $"{FormatBytes(copied)} / {FormatBytes(details.TotalBytes)}";

                var finished = details.State is "Success" or "Failed" or "Cancelled" or "PartiallyDone";
                IsCancelEnabled = !finished;
                IsOkEnabled = finished;
            }

            var logs = await _ipc.GetRunLogsAsync(RunId, ct);
            if (logs.Count > _lastLogCount)
            {
                foreach (var line in logs.Skip(_lastLogCount))
                    Logs.Add(new LogLineVm(line.At, line.Level, line.Message));

                _lastLogCount = logs.Count;
            }

            await Task.Delay(500, ct);
        }
    }

    [RelayCommand]
    public async Task CancelAsync()
    {
        if (!IsCancelEnabled)
            return;
        await _ipc.SendAsync(new AbdsCommand(AbdsCommandType.CancelRun, new() { ["runId"] = RunId }));
    }

    [RelayCommand]
    public async Task ExportLogsAsync()
    {
        var path = Path.Combine(FileSystem.Current.CacheDirectory, $"ABDS_Run_{RunId}.txt");
        await using var sw = new StreamWriter(path);
        foreach (var l in Logs)
            await sw.WriteLineAsync(l.Text);

#if WINDOWS
        System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{path}\"");
#endif
    }

    public void Stop()
    {
        try
        { _loopCts?.Cancel(); }
        catch { }
        _loopCts = null;
    }

    private static string FormatBytes(long b)
    {
        string[] u = ["B","KB","MB","GB","TB"];
        double v = b;
        int i = 0;
        while (v >= 1024 && i < u.Length - 1)
        { v /= 1024; i++; }
        return $"{v:0.##} {u[i]}";
    }
}