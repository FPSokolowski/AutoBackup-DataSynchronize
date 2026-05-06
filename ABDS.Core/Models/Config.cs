namespace ABDS.Core.Models;

public sealed record SyncPair(
    string SourcePath,
    List<string> TargetPaths,
    bool Enabled = true
)
{
    public Dictionary<string, DestinationEndpoint> TargetLocations { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed record BackupSource(
    string SourcePath,
    string BackupRootPath,
    bool Enabled = true
)
{
    public DestinationEndpoint? BackupDestination { get; init; }
}

public enum DestinationKind
{
    Unknown = 0,
    LocalPath = 1,
    RemovableDevice = 2,
    NetworkShare = 3,
    Ftp = 4
}

public sealed record DestinationEndpoint
{
    public string Location { get; init; } = "";
    public DestinationKind Kind { get; init; } = DestinationKind.Unknown;
    public DestinationIdentity? Identity { get; init; }
    public DestinationProbeResult? LastProbe { get; init; }
}

public sealed record DestinationIdentity
{
    public string Fingerprint { get; init; } = "";
    public string? RootPath { get; init; }
    public string? VolumeLabel { get; init; }
    public string? VolumeSerial { get; init; }
    public string? DriveType { get; init; }
    public string? DriveFormat { get; init; }
    public long? TotalSizeBytes { get; init; }
    public string? UriHost { get; init; }
    public string? UriPath { get; init; }
}

public sealed record DestinationProbeResult
{
    public string Location { get; init; } = "";
    public DestinationKind Kind { get; init; } = DestinationKind.Unknown;
    public bool Available { get; init; }
    public bool Writable { get; init; }
    public DateTimeOffset TestedAt { get; init; } = DateTimeOffset.Now;
    public DestinationIdentity? Identity { get; init; }
    public string Status { get; init; } = "Unknown";
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public string? DiagnosticDetails { get; init; }
}

public sealed record AbdsScheduleConfig
{
    public bool AutoSyncEnabled { get; init; } = true;
    public TimeSpan AutoSyncInterval { get; init; } = TimeSpan.FromMinutes(15);

    public SyncComparisonMode SyncComparisonMode { get; init; }
        = SyncComparisonMode.HashBelowSizeMb; // domyślne

    public int HashBelowSizeMbThreshold { get; init; } = 20; // domyślnie 20 MB

    public bool AutoBackupEnabled { get; init; } = true;
    public TimeSpan AutoBackupIntervalFromLastSuccess { get; init; }
        = TimeSpan.FromHours(12);

    public bool SyncOnAppStart { get; init; } = true;
    public bool SyncOnAppExit { get; init; } = false;

    public long MaxBackupStorageBytes { get; init; }
        = 300L * 1024 * 1024 * 1024;
}

public sealed record AbdsConfig
{
    public int ServiceTickMinutes { get; init; } = 5;

    public List<SyncPair> SyncPairs { get; init; } = new();
    public List<BackupSource> BackupSources { get; init; } = new();

    public AbdsScheduleConfig Schedule { get; init; } = new();

    // Retry reguły
    public TimeSpan RetryFailedBackupAfter { get; init; } = TimeSpan.FromMinutes(30);
    public TimeSpan RetryFailedSyncAfter { get; init; } = TimeSpan.FromMinutes(10);
    public TimeSpan RetryPartialSyncAfter { get; init; } = TimeSpan.FromMinutes(10);

    // Krytyczne przeterminowanie
    public int CriticalSyncOverdueFactor { get; init; } = 2; // 2x interwału
    public TimeSpan CriticalBackupOverdueExtra { get; init; } = TimeSpan.FromHours(2); // interwał + 2h
}
