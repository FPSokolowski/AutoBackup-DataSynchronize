using System.Collections.Concurrent;
using System.Text.Json;

namespace ABDS.Core.Hashing;

public sealed class JsonHashCache : IHashCache
{
    private readonly ConcurrentDictionary<HashCacheKey, HashCacheEntry> _map = new();
    private readonly string _filePath;

    public int MaxEntries { get; }
    public TimeSpan MaxAge { get; } // opcjonalne “starzenie”

    public JsonHashCache(string filePath, int maxEntries = 100_000, TimeSpan? maxAge = null)
    {
        _filePath = filePath;
        MaxEntries = maxEntries;
        MaxAge = maxAge ?? TimeSpan.FromDays(30);
    }

    public bool TryGet(HashCacheKey key, out string sha256Hex)
    {
        sha256Hex = default!;

        if (_map.TryGetValue(key, out var entry))
        {
            // jeśli chcesz, możesz tu odświeżać CachedAtUtc – ja celowo NIE, żeby “trim” był stabilny
            sha256Hex = entry.Sha256Hex;
            return true;
        }

        return false;
    }

    public void Set(HashCacheKey key, string sha256Hex)
    {
        var entry = new HashCacheEntry(key, sha256Hex, DateTimeOffset.UtcNow);
        _map[key] = entry;
    }

    public void TrimIfNeeded()
    {
        // 1) usuń stare wpisy
        var cutoff = DateTimeOffset.UtcNow - MaxAge;
        foreach (var kv in _map)
        {
            if (kv.Value.CachedAtUtc < cutoff)
                _map.TryRemove(kv.Key, out _);
        }

        // 2) jeśli dalej za dużo → usuń najstarsze (proste, O(n log n), ok dla tick co kilka minut)
        var count = _map.Count;
        if (count <= MaxEntries)
            return;

        var toRemove = count - MaxEntries;
        var oldest = _map.Values
            .OrderBy(v => v.CachedAtUtc)
            .Take(toRemove)
            .Select(v => v.Key)
            .ToList();

        foreach (var k in oldest)
            _map.TryRemove(k, out _);
    }

    public async Task LoadAsync(CancellationToken ct)
    {
        try
        {
            if (!File.Exists(_filePath))
                return;

            await using var fs = File.OpenRead(_filePath);
            var list = await JsonSerializer.DeserializeAsync<List<HashCacheEntry>>(fs, cancellationToken: ct);

            if (list is null)
                return;

            _map.Clear();
            foreach (var e in list)
                _map[e.Key] = e;

            TrimIfNeeded();
        }
        catch
        {
            // cache jest “best-effort” → nie wywalaj aplikacji
        }
    }

    public async Task SaveAsync(CancellationToken ct)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);

            TrimIfNeeded();

            var list = _map.Values.ToList();

            await using var fs = File.Create(_filePath);
            await JsonSerializer.SerializeAsync(fs, list, cancellationToken: ct);
        }
        catch
        {
            // best-effort
        }
    }
}