using ABDS.Core.Backup;
using ABDS.Core.Destinations;
using ABDS.Core.Hashing;
using ABDS.Core.IO;
using ABDS.Core.Models;
using ABDS.Core.Sync;
using ABDS.Service;

var tests = new (string Name, Func<Task> Body)[]
{
    ("SyncPlanner plans missing and changed files", SyncPlannerPlansMissingAndChangedFiles),
    ("Hash cache persists entries", HashCachePersistsEntries),
    ("BackupEngine creates timestamped snapshot", BackupEngineCreatesTimestampedSnapshot),
    ("FileCopyWithRetry copies atomically", FileCopyWithRetryCopiesAtomically),
    ("HashBelowSize detects content changes when metadata matches", HashBelowSizeDetectsContentChangesWhenMetadataMatches),
    ("State store returns recent runs newest first", StateStoreReturnsRecentRunsNewestFirst),
    ("Destination probe writes reads and deletes local target", DestinationProbeWritesReadsAndDeletesLocalTarget),
};

var failed = 0;

foreach (var test in tests)
{
    try
    {
        await test.Body();
        Console.WriteLine($"PASS {test.Name}");
    }
    catch (Exception ex)
    {
        failed++;
        Console.Error.WriteLine($"FAIL {test.Name}");
        Console.Error.WriteLine(ex);
    }
}

if (failed > 0)
{
    Console.Error.WriteLine($"{failed} test(s) failed.");
    return 1;
}

Console.WriteLine($"{tests.Length} test(s) passed.");
return 0;

static async Task SyncPlannerPlansMissingAndChangedFiles()
{
    using var scope = TempScope.Create();
    var source = scope.CreateDirectory("source");
    var target = scope.CreateDirectory("target");

    var sameSource = Path.Combine(source, "same.txt");
    var sameTarget = Path.Combine(target, "same.txt");
    await File.WriteAllTextAsync(sameSource, "same");
    Directory.CreateDirectory(Path.GetDirectoryName(sameTarget)!);
    await File.WriteAllTextAsync(sameTarget, "same");
    File.SetLastWriteTimeUtc(sameTarget, File.GetLastWriteTimeUtc(sameSource));

    var changedSource = Path.Combine(source, "changed.txt");
    var changedTarget = Path.Combine(target, "changed.txt");
    await File.WriteAllTextAsync(changedSource, "newer content");
    await File.WriteAllTextAsync(changedTarget, "old");

    var missingSource = Path.Combine(source, "nested", "missing.txt");
    Directory.CreateDirectory(Path.GetDirectoryName(missingSource)!);
    await File.WriteAllTextAsync(missingSource, "missing");

    var cache = new JsonHashCache(Path.Combine(scope.Root, "hashcache.json"));
    var strategy = FileComparisonFactory.Create(SyncComparisonMode.HashBelowSizeMb, 20, cache);
    var plan = SyncPlanner.BuildPlan(source, target, strategy);

    AssertEqual(2, plan.Count);
    AssertContains(plan, item => item.SourceFile == changedSource);
    AssertContains(plan, item => item.SourceFile == missingSource);
}

static async Task HashCachePersistsEntries()
{
    using var scope = TempScope.Create();
    var cachePath = Path.Combine(scope.Root, "cache", "hashcache.json");
    var key = new HashCacheKey(Path.Combine(scope.Root, "file.txt"), 10, 123);

    var cache = new JsonHashCache(cachePath);
    cache.Set(key, "ABCDEF");
    await cache.SaveAsync(CancellationToken.None);

    var loaded = new JsonHashCache(cachePath);
    await loaded.LoadAsync(CancellationToken.None);

    AssertTrue(loaded.TryGet(key, out var value));
    AssertEqual("ABCDEF", value);
}

static async Task BackupEngineCreatesTimestampedSnapshot()
{
    using var scope = TempScope.Create();
    var source = scope.CreateDirectory("source");
    var backupRoot = scope.CreateDirectory("backups");

    var file = Path.Combine(source, "nested", "model.dwg");
    Directory.CreateDirectory(Path.GetDirectoryName(file)!);
    await File.WriteAllTextAsync(file, "cad data");

    var copied = 0L;
    await BackupEngine.RunBackupAsync(
        source,
        backupRoot,
        maxStorageBytes: 1024 * 1024,
        CancellationToken.None,
        _ => Task.CompletedTask,
        _ => Task.CompletedTask,
        _ => Task.CompletedTask,
        bytes =>
        {
            copied += bytes;
            return Task.CompletedTask;
        });

    var snapshots = Directory.GetDirectories(backupRoot);
    AssertEqual(1, snapshots.Length);
    AssertTrue(File.Exists(Path.Combine(snapshots[0], "nested", "model.dwg")));
    AssertEqual(new FileInfo(file).Length, copied);
}

static async Task FileCopyWithRetryCopiesAtomically()
{
    using var scope = TempScope.Create();
    var source = Path.Combine(scope.Root, "source.txt");
    var target = Path.Combine(scope.Root, "target", "target.txt");

    await File.WriteAllTextAsync(source, "important content");
    await FileCopyWithRetry.CopyFileAtomicAsync(source, target, CancellationToken.None);

    AssertEqual("important content", await File.ReadAllTextAsync(target));
    AssertEqual(0, Directory.GetFiles(Path.GetDirectoryName(target)!, "*.abds_tmp_*").Length);
}

static async Task HashBelowSizeDetectsContentChangesWhenMetadataMatches()
{
    using var scope = TempScope.Create();
    var source = Path.Combine(scope.Root, "source.bin");
    var target = Path.Combine(scope.Root, "target.bin");

    await File.WriteAllTextAsync(source, "abcd");
    await File.WriteAllTextAsync(target, "wxyz");
    File.SetLastWriteTimeUtc(target, File.GetLastWriteTimeUtc(source));

    var cache = new JsonHashCache(Path.Combine(scope.Root, "hashcache.json"));
    var strategy = FileComparisonFactory.Create(SyncComparisonMode.HashBelowSizeMb, 20, cache);

    AssertTrue(strategy.AreDifferent(new FileInfo(source), new FileInfo(target)));
}

static async Task StateStoreReturnsRecentRunsNewestFirst()
{
    var store = new AbdsStateStore();
    var older = store.CreateRun("older-run", AbdsJobRequest.Backup("D:\\SourceA", "F:\\Backups", "test"));
    older.Complete("Success", "Done");
    older.Commit();

    await Task.Delay(15);
    var newer = store.CreateRun("newer-run", AbdsJobRequest.Sync("D:\\SourceB", ["E:\\Target"], "test"));
    newer.Complete("Failed", "Broken");
    newer.Commit();

    var runs = store.GetRecentRuns(2);

    AssertEqual(2, runs.Count);
    AssertEqual("newer-run", runs[0].RunId);
    AssertEqual("older-run", runs[1].RunId);
}

static async Task DestinationProbeWritesReadsAndDeletesLocalTarget()
{
    using var scope = TempScope.Create();
    var target = scope.CreateDirectory("destination");

    var result = await DestinationProbe.ProbeAsync(target, writeTest: true, CancellationToken.None);

    AssertTrue(result.Available, result.DiagnosticDetails ?? result.ErrorMessage);
    AssertTrue(result.Writable, result.DiagnosticDetails ?? result.ErrorMessage);
    AssertTrue(result.Identity?.Fingerprint.Length > 0);
    AssertEqual(0, Directory.GetFiles(target, ".abds_probe_*").Length);
}

static void AssertTrue(bool condition, string? message = null)
{
    if (!condition)
        throw new InvalidOperationException(message ?? "Expected condition to be true.");
}

static void AssertEqual<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new InvalidOperationException($"Expected {expected}, got {actual}.");
}

static void AssertContains<T>(IEnumerable<T> values, Func<T, bool> predicate)
{
    if (!values.Any(predicate))
        throw new InvalidOperationException("Expected collection to contain a matching item.");
}

internal sealed class TempScope : IDisposable
{
    public string Root { get; }

    private TempScope(string root)
    {
        Root = root;
    }

    public static TempScope Create()
    {
        var root = Path.Combine(Path.GetTempPath(), "ABDS_CoreTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return new TempScope(root);
    }

    public string CreateDirectory(string name)
    {
        var path = Path.Combine(Root, name);
        Directory.CreateDirectory(path);
        return path;
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
        catch
        {
            // Best-effort cleanup; failed cleanup should not mask test results.
        }
    }
}
