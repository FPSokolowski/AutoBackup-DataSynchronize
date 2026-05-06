namespace ABDS.Core.Models;

public sealed record AbdsTaskResult(
    Guid RunId,
    AbdsTaskType Type,
    AbdsTaskState State,
    DateTimeOffset StartedAt,
    DateTimeOffset? FinishedAt,
    string? Summary,
    List<string> Warnings,
    List<string> Errors,
    List<string> PartiallySkippedFiles
);

public sealed record AbdsPairLastStatus(
    string SourcePath,
    string TargetPath,
    AbdsTaskType Type,
    DateTimeOffset? LastSuccessAt,
    DateTimeOffset? LastAttemptAt,
    AbdsTaskState? LastState,
    string? LastErrorCode,
    string? LastErrorMessage
);