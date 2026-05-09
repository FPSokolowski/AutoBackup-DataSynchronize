namespace ABDS.Core.Models;

public enum AbdsTaskType { Backup, Sync }
public enum AbdsTaskState { Pending, Running, Success, Failed, PartiallyDone, RetryWaiting, Cancelled }
public enum BackupArchiveFormat { Zip, TarGz }
public enum BackupCompressionPreset { NoCompression, Fastest, Optimal, SmallestSize }

public enum AbdsTraySeverity
{
    Busy,
    Ok,
    Warning,
    Critical
}
