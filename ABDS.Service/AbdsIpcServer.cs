using System.Text.Json;
using ABDS.SharedIpc;
using Grpc.Core;

namespace ABDS.Service;

public sealed class AbdsIpcServer(AbdsStateStore store, AbdsWorkerFacade facade, ILogger<AbdsIpcServer> log)
    : AbdsIpcGrpc.AbdsIpcGrpcBase
{
    public override async Task<GrpcAbdsCommandResponse> Send(GrpcAbdsCommand request, ServerCallContext context)
    {
        try
        {
            var command = request.ToContract();
            var response = await HandleAsync(command, context.CancellationToken);
            return response.ToGrpc();
        }
        catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            log.LogError(ex, "gRPC client error");
            return new AbdsCommandResponse(false, ex.Message).ToGrpc();
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
                    var runId = cmd.Args?.GetValueOrDefault("runId");
                    if (string.IsNullOrWhiteSpace(runId))
                        return new(false, "runId required");

                    var run = store.GetRun(runId);
                    return run is null
                        ? new(false, "run not found")
                        : new(true, JsonSerializer.Serialize(run));
                }

            case AbdsCommandType.GetRunLogs:
                {
                    var runId = cmd.Args?.GetValueOrDefault("runId");
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
                    var src = cmd.Args?.GetValueOrDefault("sourcePath");
                    var dst = cmd.Args?.GetValueOrDefault("targetPath");
                    if (string.IsNullOrWhiteSpace(src) || string.IsNullOrWhiteSpace(dst))
                        return new(false, "sourcePath & targetPath required");

                    var runId = await facade.EnqueueForceSyncPairAsync(src, dst, ct);
                    return new(true, "Scheduled sync pair", runId);
                }

            case AbdsCommandType.ForceBackupSource:
                {
                    var src = cmd.Args?.GetValueOrDefault("sourcePath");
                    var root = cmd.Args?.GetValueOrDefault("backupRootPath");
                    if (string.IsNullOrWhiteSpace(src) || string.IsNullOrWhiteSpace(root))
                        return new(false, "sourcePath & backupRootPath required");

                    var runId = await facade.EnqueueForceBackupSourceAsync(src, root, ct);
                    return new(true, "Scheduled backup source", runId);
                }

            case AbdsCommandType.CancelRun:
                {
                    var runId = cmd.Args?.GetValueOrDefault("runId");
                    if (string.IsNullOrWhiteSpace(runId))
                        return new(false, "runId required");

                    facade.Cancel(runId);
                    return new(true, "Cancel requested");
                }

            case AbdsCommandType.OpenGui:
                return new(true, "OK");

            default:
                return new(false, "Unknown command");
        }
    }
}
