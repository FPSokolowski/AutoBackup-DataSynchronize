using ABDS.Core.IO;

namespace ABDS.Core.Backup;

public static class BackupEngine
{
    public static async Task RunBackupAsync(
        string sourcePath,
        string backupRoot,
        long maxStorageBytes,
        CancellationToken ct,
        Func<string, Task> info,
        Func<string, Task> warn,
        Func<string, Task> error,
        Func<long, Task>? copiedBytes = null)
    {
        if (!Directory.Exists(sourcePath))
            throw new DirectoryNotFoundException(sourcePath);

        Directory.CreateDirectory(backupRoot);

        var stamp = DateTimeOffset.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var snapshotRoot = Path.Combine(backupRoot, stamp);
        Directory.CreateDirectory(snapshotRoot);

        await info($"Backup snapshot: {snapshotRoot}");
        await info($"Source: {sourcePath}");

        foreach (var srcFile in Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories))
        {
            ct.ThrowIfCancellationRequested();

            var rel = Path.GetRelativePath(sourcePath, srcFile);
            var dstFile = Path.Combine(snapshotRoot, rel);

            await FileCopyWithRetry.CopyFileAtomicAsync(srcFile, dstFile, ct, info, warn, error);

            // zachowaj LastWriteTimeUtc, żeby przydatne w późniejszym porównaniu
            var sInfo = new FileInfo(srcFile);
            File.SetLastWriteTimeUtc(dstFile, sInfo.LastWriteTimeUtc);

            if (copiedBytes is not null)
                await copiedBytes(sInfo.Length);
        }

        await info("Backup complete. Applying retention...");

        ApplyRetention(backupRoot, maxStorageBytes, info).GetAwaiter().GetResult();
    }

    private static async Task ApplyRetention(string backupRoot, long maxBytes, Func<string, Task> info)
    {
        long TotalBytes()
        {
            if (!Directory.Exists(backupRoot))
                return 0;
            long sum = 0;
            foreach (var file in Directory.EnumerateFiles(backupRoot, "*", SearchOption.AllDirectories))
                sum += new FileInfo(file).Length;
            return sum;
        }

        var total = TotalBytes();
        if (total <= maxBytes)
        {
            await info($"Retention OK. Total={FormatBytes(total)} <= Max={FormatBytes(maxBytes)}");
            return;
        }

        // usuń najstarsze snapshoty (katalogi) aż zejdziemy poniżej limitu
        var dirs = new DirectoryInfo(backupRoot)
            .EnumerateDirectories()
            .OrderBy(d => d.CreationTimeUtc)
            .ToList();

        foreach (var d in dirs)
        {
            if (total <= maxBytes)
                break;

            await info($"Retention delete: {d.FullName}");
            try
            {
                d.Delete(recursive: true);
            }
            catch { /* log ewentualnie */ }

            total = TotalBytes();
        }

        await info($"Retention done. Total={FormatBytes(total)}");
    }

    private static string FormatBytes(long b)
    {
        string[] u = ["B","KB","MB","GB","TB"];
        double v = b;
        int i = 0;
        while (v >= 1024 && i < u.Length - 1)
        { v /= 1024; i++; }
        return $"{v:0.##} {u[i]}";
    }

    public static long CalculateTotalBytes(string sourcePath)
    {
        long total = 0;
        foreach (var f in Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories))
            total += new FileInfo(f).Length;
        return total;
    }
}
