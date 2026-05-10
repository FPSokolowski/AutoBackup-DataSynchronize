using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using ABDS.SharedIpc;

namespace ABDS.Service;

public sealed class AbdsIpcServer(AbdsStateStore store, AbdsWorkerFacade facade, ILogger<AbdsIpcServer> log)
{
    private CancellationToken _ct;
    private static readonly TimeSpan ClientTimeout = TimeSpan.FromSeconds(20);

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
            NamedPipeServerStream? pipe = null;
            try
            {
                pipe = new NamedPipeServerStream(
                    AbdsIpc.PipeName,
                    PipeDirection.InOut,
                    maxNumberOfServerInstances: 10,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                await pipe.WaitForConnectionAsync(_ct);
                var connectedPipe = pipe;
                pipe = null;

                _ = Task.Run(() => HandleClientAsync(connectedPipe), CancellationToken.None);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                log.LogError(ex, "IPC error");
            }
            finally
            {
                pipe?.Dispose();
            }
        }
    }

    private async Task HandleClientAsync(NamedPipeServerStream pipe)
    {
        using (pipe)
        using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(_ct))
        {
            timeout.CancelAfter(ClientTimeout);
            var ct = timeout.Token;

            try
            {
                using var sr = new StreamReader(pipe, Encoding.UTF8, leaveOpen: true);
                var line = await sr.ReadLineAsync(ct);
                if (line is null)
                    return;

                var cmd = JsonSerializer.Deserialize<AbdsCommand>(line);
                var resp = cmd is null
                    ? new AbdsCommandResponse(false, "Invalid command.")
                    : await HandleAsync(cmd, ct);

                await WriteResponseAsync(pipe, resp, ct);
            }
            catch (OperationCanceledException) when (_ct.IsCancellationRequested)
            {
                // Service is shutting down.
            }
            catch (OperationCanceledException ex)
            {
                log.LogWarning(ex, "IPC client timed out");
                await TryWriteResponseAsync(pipe, new AbdsCommandResponse(false, "IPC request timed out."));
            }
            catch (JsonException ex)
            {
                log.LogWarning(ex, "Invalid IPC payload");
                await TryWriteResponseAsync(pipe, new AbdsCommandResponse(false, "Invalid command payload."));
            }
            catch (Exception ex)
            {
                log.LogError(ex, "IPC client error");
                await TryWriteResponseAsync(pipe, new AbdsCommandResponse(false, ex.Message));
            }
        }
    }

    private static async Task WriteResponseAsync(PipeStream pipe, AbdsCommandResponse response, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(response);
        var bytes = Encoding.UTF8.GetBytes(json + "\n");
        await pipe.WriteAsync(bytes, 0, bytes.Length, ct);
        await pipe.FlushAsync(ct);
    }

    private static async Task TryWriteResponseAsync(PipeStream pipe, AbdsCommandResponse response)
    {
        try
        {
            if (pipe.IsConnected)
                await WriteResponseAsync(pipe, response, CancellationToken.None);
        }
        catch
        {
            // The client may have already disconnected.
        }
    }

    private async Task<AbdsCommandResponse> HandleAsync(AbdsCommand cmd, CancellationToken ct)
    {
        var cfg = await store.LoadConfigAsync(ct);

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
                    var runId = await facade.EnqueueForceSyncAllAsync(ct);
                    return new(true, "Scheduled sync all", runId);
                }

            case AbdsCommandType.ForceBackupAll:
                {
                    var runId = await facade.EnqueueForceBackupAllAsync(ct);
                    return new(true, "Scheduled backup all", runId);
                }

            case AbdsCommandType.ForceSyncPair:
                {
                    var src = cmd.Args?["sourcePath"];
                    var dst = cmd.Args?["targetPath"];
                    if (string.IsNullOrWhiteSpace(src) || string.IsNullOrWhiteSpace(dst))
                        return new(false, "sourcePath & targetPath required");

                    var runId = await facade.EnqueueForceSyncPairAsync(src!, dst!, ct);
                    return new(true, "Scheduled sync pair", runId);
                }

            case AbdsCommandType.ForceBackupSource:
                {
                    var src = cmd.Args?["sourcePath"];
                    var root = cmd.Args?["backupRootPath"];
                    if (string.IsNullOrWhiteSpace(src) || string.IsNullOrWhiteSpace(root))
                        return new(false, "sourcePath & backupRootPath required");

                    var runId = await facade.EnqueueForceBackupSourceAsync(src!, root!, ct);
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
