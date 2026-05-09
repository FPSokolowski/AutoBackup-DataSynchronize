using System.Formats.Tar;
using System.IO.Compression;
using ABDS.Core.Models;

namespace ABDS.Core.Backup;

public static class BackupEngine
{
    public static async Task RunBackupAsync(
        string sourcePath,
        string backupRoot,
        string backupName,
        BackupArchiveFormat archiveFormat,
        BackupCompressionPreset compressionPreset,
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
        var safeName = SanitizeFileName(string.IsNullOrWhiteSpace(backupName) ? new DirectoryInfo(sourcePath).Name : backupName);
        var extension = archiveFormat == BackupArchiveFormat.TarGz ? ".tar.gz" : ".zip";
        var archivePath = Path.Combine(backupRoot, $"{stamp}_{safeName}{extension}");
        var tempArchivePath = archivePath + ".abds_tmp";

        if (File.Exists(tempArchivePath))
            File.Delete(tempArchivePath);

        await info($"Backup archive: {archivePath}");
        await info($"Source: {sourcePath}");
        await info($"Compression: {archiveFormat} / {compressionPreset}");

        try
        {
            if (archiveFormat == BackupArchiveFormat.TarGz)
                await CreateTarGzArchiveAsync(sourcePath, tempArchivePath, ToCompressionLevel(compressionPreset), ct, copiedBytes);
            else
                await CreateZipArchiveAsync(sourcePath, tempArchivePath, ToCompressionLevel(compressionPreset), ct, copiedBytes);

            File.Move(tempArchivePath, archivePath, overwrite: false);
        }
        catch
        {
            try
            {
                if (File.Exists(tempArchivePath))
                    File.Delete(tempArchivePath);
            }
            catch { }

            throw;
        }

        await info("Backup complete. Applying retention...");

        await ApplyRetention(backupRoot, maxStorageBytes, info);
    }

    private static async Task CreateZipArchiveAsync(
        string sourcePath,
        string archivePath,
        CompressionLevel compressionLevel,
        CancellationToken ct,
        Func<long, Task>? copiedBytes)
    {
        using var archive = ZipFile.Open(archivePath, ZipArchiveMode.Create);
        foreach (var srcFile in Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories))
        {
            ct.ThrowIfCancellationRequested();
            var rel = Path.GetRelativePath(sourcePath, srcFile);
            archive.CreateEntryFromFile(srcFile, rel, compressionLevel);

            if (copiedBytes is not null)
                await copiedBytes(new FileInfo(srcFile).Length);
        }
    }

    private static async Task CreateTarGzArchiveAsync(
        string sourcePath,
        string archivePath,
        CompressionLevel compressionLevel,
        CancellationToken ct,
        Func<long, Task>? copiedBytes)
    {
        await using (var file = File.Create(archivePath))
        await using (var gzip = new GZipStream(file, compressionLevel))
        {
            TarFile.CreateFromDirectory(sourcePath, gzip, includeBaseDirectory: false);
        }

        if (copiedBytes is null)
            return;

        foreach (var srcFile in Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories))
        {
            ct.ThrowIfCancellationRequested();
            await copiedBytes(new FileInfo(srcFile).Length);
        }
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

        var root = new DirectoryInfo(backupRoot);
        var items = root.EnumerateDirectories()
            .Cast<FileSystemInfo>()
            .Concat(root.EnumerateFiles("*.zip"))
            .Concat(root.EnumerateFiles("*.tar.gz"))
            .OrderBy(x => x.CreationTimeUtc)
            .ToList();

        foreach (var item in items)
        {
            if (total <= maxBytes)
                break;

            await info($"Retention delete: {item.FullName}");
            try
            {
                if (item is DirectoryInfo dir)
                    dir.Delete(recursive: true);
                else
                    item.Delete();
            }
            catch { }

            total = TotalBytes();
        }

        await info($"Retention done. Total={FormatBytes(total)}");
    }

    private static CompressionLevel ToCompressionLevel(BackupCompressionPreset preset)
        => preset switch
        {
            BackupCompressionPreset.NoCompression => CompressionLevel.NoCompression,
            BackupCompressionPreset.Fastest => CompressionLevel.Fastest,
            BackupCompressionPreset.SmallestSize => CompressionLevel.SmallestSize,
            _ => CompressionLevel.Optimal
        };

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = name.Trim().Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray();
        var sanitized = new string(chars).Trim('_', '.', ' ');
        return string.IsNullOrWhiteSpace(sanitized) ? "backup" : sanitized;
    }

    private static string FormatBytes(long b)
    {
        string[] u = ["B", "KB", "MB", "GB", "TB"];
        double v = b;
        var i = 0;
        while (v >= 1024 && i < u.Length - 1)
        {
            v /= 1024;
            i++;
        }

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
