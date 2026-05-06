namespace ABDS.SharedIpc;

public static class AbdsIpc
{
    public const string PipeName = "ABDS_PIPE_V1";
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
