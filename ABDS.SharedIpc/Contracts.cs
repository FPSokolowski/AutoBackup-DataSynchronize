namespace ABDS.SharedIpc;

public static class AbdsIpc
{
    public const string DefaultGrpcEndpoint = "http://127.0.0.1:5077";

    public static string GrpcEndpoint =>
        Environment.GetEnvironmentVariable("ABDS_GRPC_ENDPOINT") ?? DefaultGrpcEndpoint;
}

public enum AbdsCommandType
{
    GetStatus,
    GetRecentRuns,
    GetRunDetails,
    GetRunLogs,

    ForceSyncAll,
    ForceSyncPair,
    ForceBackupAll,
    ForceBackupSource,

    CancelRun,
    OpenGui
}

public sealed record AbdsCommand(
    AbdsCommandType Type,
    Dictionary<string, string>? Args = null
);

public sealed record AbdsCommandResponse(
    bool Ok,
    string Message,
    string? RunId = null
);

public static class AbdsCommandGrpcMapper
{
    public static GrpcAbdsCommand ToGrpc(this AbdsCommand command)
    {
        var grpc = new GrpcAbdsCommand
        {
            Type = command.Type.ToGrpc()
        };

        if (command.Args is not null)
        {
            foreach (var (key, value) in command.Args)
                grpc.Args[key] = value;
        }

        return grpc;
    }

    public static AbdsCommand ToContract(this GrpcAbdsCommand command)
        => new(command.Type.ToContract(), command.Args.ToDictionary(x => x.Key, x => x.Value));

    public static AbdsCommandResponse ToContract(this GrpcAbdsCommandResponse response)
        => new(response.Ok, response.Message, string.IsNullOrWhiteSpace(response.RunId) ? null : response.RunId);

    public static GrpcAbdsCommandResponse ToGrpc(this AbdsCommandResponse response)
        => new()
        {
            Ok = response.Ok,
            Message = response.Message,
            RunId = response.RunId ?? ""
        };

    private static GrpcAbdsCommandType ToGrpc(this AbdsCommandType type)
        => type switch
        {
            AbdsCommandType.GetStatus => GrpcAbdsCommandType.GetStatus,
            AbdsCommandType.GetRecentRuns => GrpcAbdsCommandType.GetRecentRuns,
            AbdsCommandType.GetRunDetails => GrpcAbdsCommandType.GetRunDetails,
            AbdsCommandType.GetRunLogs => GrpcAbdsCommandType.GetRunLogs,
            AbdsCommandType.ForceSyncAll => GrpcAbdsCommandType.ForceSyncAll,
            AbdsCommandType.ForceSyncPair => GrpcAbdsCommandType.ForceSyncPair,
            AbdsCommandType.ForceBackupAll => GrpcAbdsCommandType.ForceBackupAll,
            AbdsCommandType.ForceBackupSource => GrpcAbdsCommandType.ForceBackupSource,
            AbdsCommandType.CancelRun => GrpcAbdsCommandType.CancelRun,
            AbdsCommandType.OpenGui => GrpcAbdsCommandType.OpenGui,
            _ => GrpcAbdsCommandType.Unspecified
        };

    private static AbdsCommandType ToContract(this GrpcAbdsCommandType type)
        => type switch
        {
            GrpcAbdsCommandType.GetStatus => AbdsCommandType.GetStatus,
            GrpcAbdsCommandType.GetRecentRuns => AbdsCommandType.GetRecentRuns,
            GrpcAbdsCommandType.GetRunDetails => AbdsCommandType.GetRunDetails,
            GrpcAbdsCommandType.GetRunLogs => AbdsCommandType.GetRunLogs,
            GrpcAbdsCommandType.ForceSyncAll => AbdsCommandType.ForceSyncAll,
            GrpcAbdsCommandType.ForceSyncPair => AbdsCommandType.ForceSyncPair,
            GrpcAbdsCommandType.ForceBackupAll => AbdsCommandType.ForceBackupAll,
            GrpcAbdsCommandType.ForceBackupSource => AbdsCommandType.ForceBackupSource,
            GrpcAbdsCommandType.CancelRun => AbdsCommandType.CancelRun,
            GrpcAbdsCommandType.OpenGui => AbdsCommandType.OpenGui,
            _ => throw new InvalidOperationException("Unknown ABDS gRPC command type.")
        };
}

// status snapshot dla GUI
public sealed record AbdsStatusSnapshotDto(
    string TraySeverity,
    string TrayTooltip,
    bool HasRunningJob,
    string? RunningRunId,
    List<AbdsPairStatusDto> SyncStatuses,
    List<AbdsBackupStatusDto> BackupStatuses,
    List<AbdsDestinationStatusDto> DestinationStatuses
);

public sealed record AbdsDestinationStatusDto(
    string Location,
    string Kind,
    bool Available,
    bool Writable,
    string Status,
    string? ErrorMessage,
    DateTimeOffset TestedAt
);

public sealed record AbdsPairStatusDto(
    string SourcePath,
    string TargetPath,
    string? LastState,
    DateTimeOffset? LastAttemptAt,
    DateTimeOffset? LastSuccessAt,
    string? LastErrorCode,
    string? LastErrorMessage
);

public sealed record AbdsBackupStatusDto(
    string SourcePath,
    string BackupRootPath,
    string? LastState,
    DateTimeOffset? LastAttemptAt,
    DateTimeOffset? LastSuccessAt,
    string? LastErrorCode,
    string? LastErrorMessage
);

public sealed record AbdsRunDetailsDto(
    string RunId,
    string TaskType,
    string State,
    DateTimeOffset StartedAt,
    DateTimeOffset? FinishedAt,
    string? Summary,
    long TotalBytes,
    long CopiedBytes,
    List<string> Sources,
    List<string> Targets,
    List<string> PartiallySkippedFiles,
    List<string> Errors
);

public sealed record AbdsRunLogLineDto(
    DateTimeOffset At,
    string Level, // INFO/WARN/ERROR/SUCCESS
    string Message
);
