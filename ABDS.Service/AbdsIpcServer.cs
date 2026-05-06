using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using ABDS.SharedIpc;

namespace ABDS.Service;

public sealed class AbdsIpcServer(AbdsStateStore store, AbdsWorkerFacade facade, ILogger<AbdsIpcServer> log)
{
    private CancellationToken _ct;

    public Task StartAsync(CancellationToken ct)
    {
        _ct = ct;
        _ = Task.Run(ListenLoopAsync, ct);
        return Task.CompletedTask;
    }

    private async Task ListenLoopAsync()
    {
        while (!_ct.IsCancellationRequested)
        {
            try
            {
                using var pipe = new NamedPipeServerStream(
                    AbdsIpc.PipeName,
                    PipeDirection.InOut,
                    maxNumberOfServerInstances: 10,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                await pipe.WaitForConnectionAsync(_ct);

                using var sr = new StreamReader(pipe, Encoding.UTF8, leaveOpen: true);
                var line = await sr.ReadLineAsync(_ct);
                if (line is null)
                    continue;

                var cmd = JsonSerializer.Deserialize<AbdsCommand>(line);
                var resp = await HandleAsync(cmd!);

                var json = JsonSerializer.Serialize(resp);
                var bytes = Encoding.UTF8.GetBytes(json + "\n");
                await pipe.WriteAsync(bytes, 0, bytes.Length, _ct);
                await pipe.FlushAsync(_ct);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                log.LogError(ex, "IPC error");
            }
        }
    }

    private async Task<AbdsCommandResponse> HandleAsync(AbdsCommand cmd)
    {
        var cfg = await store.LoadConfigAsync(_ct);

        switch (cmd.Type)
        {
            case AbdsCommandType.GetStatus:
                {
                    store.UpdateTrayState(cfg);
                    var snap = store.BuildStatusSnapshot(cfg);
                    return new AbdsCommandResponse(true, JsonSerializer.Serialize(snap));
                }

            case AbdsCommandType.GetRecentRuns:
                {
                    var takeArg = cmd.Args?.GetValueOrDefault("take");
                    var take = int.TryParse(takeArg, out var parsed) ? parsed : 50;
                    var runs = store.GetRecentRuns(take);
                    return new AbdsCommandResponse(true, JsonSerializer.Serialize(runs));
                }

            case AbdsCommandType.GetRunDetails:
                {
                    var runId = cmd.Args?["runId"];
                    if (string.IsNullOrWhiteSpace(runId))
                        return new(false, "runId required");

                    var run = store.GetRun(runId);
                    return run is null
                        ? new(false, "run not found")
                        : new(true, JsonSerializer.Serialize(run));
                }

            case AbdsCommandType.GetRunLogs:
                {
                    var runId = cmd.Args?["runId"];
                    if (string.IsNullOrWhiteSpace(runId))
                        return new(false, "runId required");

                    var logs = store.GetRunLogs(runId);
                    return new(true, JsonSerializer.Serialize(logs));
                }

            case AbdsCommandType.ForceSyncAll:
                {
                    var runId = await facade.EnqueueForceSyncAllAsync(_ct);
                    return new(true, "Scheduled sync all", runId);
                }

            case AbdsCommandType.ForceBackupAll:
                {
                    var runId = await facade.EnqueueForceBackupAllAsync(_ct);
                    return new(true, "Scheduled backup all", runId);
                }

            case AbdsCommandType.ForceSyncPair:
                {
                    var src = cmd.Args?["sourcePath"];
                    var dst = cmd.Args?["targetPath"];
                    if (string.IsNullOrWhiteSpace(src) || string.IsNullOrWhiteSpace(dst))
                        return new(false, "sourcePath & targetPath required");

                    var runId = await facade.EnqueueForceSyncPairAsync(src!, dst!, _ct);
                    return new(true, "Scheduled sync pair", runId);
                }

            case AbdsCommandType.ForceBackupSource:
                {
                    var src = cmd.Args?["sourcePath"];
                    var root = cmd.Args?["backupRootPath"];
                    if (string.IsNullOrWhiteSpace(src) || string.IsNullOrWhiteSpace(root))
                        return new(false, "sourcePath & backupRootPath required");

                    var runId = await facade.EnqueueForceBackupSourceAsync(src!, root!, _ct);
                    return new(true, "Scheduled backup source", runId);
                }

            case AbdsCommandType.CancelRun:
                {
                    var runId = cmd.Args?["runId"];
                    if (string.IsNullOrWhiteSpace(runId))
                        return new(false, "runId required");

                    facade.Cancel(runId!);
                    return new(true, "Cancel requested");
                }

            case AbdsCommandType.OpenGui:
                // soft request - GUI/CLI i tak odpali ABDS.App.exe
                return new(true, "OK");

            default:
                return new(false, "Unknown command");
        }
    }
}
