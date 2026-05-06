using System.Reflection;
using ABDS.SharedIpc;

namespace ABDS.Service;

public static class FailureDumpWriter
{
    public static async Task<string?> TryWriteFailureDumpAsync(
        string dumpsDir,
        AbdsRunDetailsDto run,
        Exception ex,
        IReadOnlyList<AbdsRunLogLineDto> logs,
        CancellationToken ct)
    {
        try
        {
            Directory.CreateDirectory(dumpsDir);

            var stamp = DateTimeOffset.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var file = Path.Combine(dumpsDir, $"{stamp}_ABDS_failureDump.txt");

            var ver = GetAppVersion();

            await using var fs = new FileStream(file, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
            await using var sw = new StreamWriter(fs);

            await sw.WriteLineAsync("=== ABDS FAILURE DUMP ===");
            await sw.WriteLineAsync($"AppVersion: {ver}");
            await sw.WriteLineAsync($"Machine: {Environment.MachineName}");
            await sw.WriteLineAsync($"OS: {Environment.OSVersion}");
            await sw.WriteLineAsync($"Now: {DateTimeOffset.Now:O}");
            await sw.WriteLineAsync("");

            await sw.WriteLineAsync("=== RUN ===");
            await sw.WriteLineAsync($"RunId: {run.RunId}");
            await sw.WriteLineAsync($"TaskType: {run.TaskType}");
            await sw.WriteLineAsync($"State: {run.State}");
            await sw.WriteLineAsync($"StartedAt: {run.StartedAt:O}");
            await sw.WriteLineAsync($"FinishedAt: {( run.FinishedAt.HasValue ? run.FinishedAt.Value.ToString("O") : "-" )}");
            await sw.WriteLineAsync($"Summary: {run.Summary ?? "-"}");
            await sw.WriteLineAsync($"Bytes: {run.CopiedBytes}/{run.TotalBytes}");
            await sw.WriteLineAsync("");

            await sw.WriteLineAsync("=== PATHS ===");
            await sw.WriteLineAsync("Sources:");
            foreach (var s in run.Sources)
                await sw.WriteLineAsync($"  - {s}");
            await sw.WriteLineAsync("Targets:");
            foreach (var t in run.Targets)
                await sw.WriteLineAsync($"  - {t}");
            await sw.WriteLineAsync("");

            await sw.WriteLineAsync("=== LAST ERROR ===");
            await sw.WriteLineAsync(ex.ToString());
            await sw.WriteLineAsync("");

            await sw.WriteLineAsync("=== RUN LOGS ===");
            foreach (var line in logs)
                await sw.WriteLineAsync($"{line.At:O} [{line.Level}] {line.Message}");

            await sw.FlushAsync(ct);
            return file;
        }
        catch
        {
            return null;
        }
    }

    private static string GetAppVersion()
    {
        try
        {
            var asm = Assembly.GetExecutingAssembly();
            return asm.GetName().Version?.ToString() ?? "unknown";
        }
        catch { return "unknown"; }
    }
}