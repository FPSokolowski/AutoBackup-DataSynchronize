using System.ComponentModel;

namespace ABDS.Core.IO;

public static class FileCopyWithRetry
{
    public static async Task CopyFileAtomicAsync(
        string sourceFile,
        string targetFile,
        CancellationToken ct,
        Func<string, Task>? logInfo = null,
        Func<string, Task>? logWarn = null,
        Func<string, Task>? logError = null)
    {
        // 2 próby: pierwsza + jedna ponowna po 1s
        for (var attempt = 1; attempt <= 2; attempt++)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                var targetDir = Path.GetDirectoryName(targetFile)!;
                Directory.CreateDirectory(targetDir);

                // atomic: kopiuj do temp, potem move z overwrite
                var temp = targetFile + ".abds_tmp_" + Guid.NewGuid().ToString("N");

                await using (var src = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                await using (var dst = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    await src.CopyToAsync(dst, 1024 * 1024, ct);
                }

                File.Move(temp, targetFile, overwrite: true);

                if (logInfo != null)
                    await logInfo($"Copied: {sourceFile} -> {targetFile}");
                return;
            }
            catch (Exception ex) when (attempt == 1 && IsTransientFileIo(ex))
            {
                if (logWarn != null)
                    await logWarn($"Transient IO (attempt 1): {sourceFile} -> {targetFile}. Retrying in 1s. {ex.Message}");
                await Task.Delay(1000, ct);
                continue;
            }
            catch (Exception ex)
            {
                if (logError != null)
                    await logError($"Copy failed: {sourceFile} -> {targetFile}. {ex}");
                throw;
            }
        }
    }

    // W praktyce: sharing violation / file in use / some IO glitches
    private static bool IsTransientFileIo(Exception ex)
    {
        if (ex is IOException)
            return true;

        if (ex is Win32Exception w32)
        {
            // m.in. ERROR_SHARING_VIOLATION (32), ERROR_LOCK_VIOLATION (33)
            return w32.NativeErrorCode is 32 or 33;
        }

        if (ex is UnauthorizedAccessException)
            return true; // czasem antywirus/ACL w trakcie
        return false;
    }
}
