namespace ABDS.Core.Hashing;

public interface IHashCache
{
    bool TryGet(HashCacheKey key, out string sha256Hex);
    void Set(HashCacheKey key, string sha256Hex);

    // housekeeping
    void TrimIfNeeded();

    // persistence
    Task LoadAsync(CancellationToken ct);
    Task SaveAsync(CancellationToken ct);
}