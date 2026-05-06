namespace ABDS.Core.Hashing;

public sealed record HashCacheKey(
    string FullPath,
    long Length,
    long LastWriteTimeUtcTicks
);

public sealed record HashCacheEntry(
    HashCacheKey Key,
    string Sha256Hex,
    DateTimeOffset CachedAtUtc
);