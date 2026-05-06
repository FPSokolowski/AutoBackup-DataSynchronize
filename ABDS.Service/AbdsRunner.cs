using ABDS.Core.Backup;
using ABDS.Core.Hashing;
using ABDS.Core.Models;
using ABDS.Core.Sync;
using ABDS.SharedIpc;

namespace ABDS.Service;

public sealed class AbdsRunner
{
    private readonly AbdsStateStore _store;
    private readonly ILogger<AbdsRunner> _log;
    private readonly IHashCache _hashCache;

    public AbdsRunner(
        AbdsStateStore store,
        ILogger<AbdsRunner> log,
        IHashCache hashCache)
    {
        _store = store;
        _log = log;
        _hashCache = hashCache;
    }

    public async Task<AbdsRunDetailsDto> RunJobAsync(
        AbdsJobRequest job,
        AbdsConfig cfg,
        CancellationToken ct)
    {
        var runId = string.IsNullOrWhiteSpace(job.RequestedRunId)
            ? Guid.NewGuid().ToString("N")
            : job.RequestedRunId;

        var runCtx = _store.CreateRun(runId, job);
        _store.SetRunning(runId);

        try
        {
            _store.AppendLog(runId, "INFO", $"Run started: {job.Type} ({job.Reason})");

            if (job.Type == AbdsTaskType.Sync)
            {
                await ExecuteSyncAsync(runCtx, job, cfg, ct);
            }
            else
            {
                await ExecuteBackupAsync(runCtx, job, cfg, ct);
            }

            if (runCtx.Run.State == "Running")
            {
                runCtx = FinalizeSuccess(runCtx);
            }

            _store.UpsertRun(runCtx.Run);
            _store.AppendLog(runId, "SUCCESS", $"Run finished: {runCtx.Run.State}");

            return runCtx.Run;
        }
        catch (OperationCanceledException)
        {
            var cancelled = runCtx.Run with
            {
                State = "Cancelled",
                FinishedAt = DateTimeOffset.Now,
                Summary = "Cancelled by user."
            };

            _store.UpsertRun(cancelled);
            _store.AppendLog(runId, "WARN", "Cancelled.");

            return cancelled;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Run failed");

            var failed = runCtx.Run with
            {
                State = "Failed",
                FinishedAt = DateTimeOffset.Now,
                Summary = ex.Message,
                Errors = runCtx.Run.Errors.Concat(new[] { ex.Message }).ToList()
            };

            _store.UpsertRun(failed);
            _store.AppendLog(runId, "ERROR", ex.ToString());

            var dumpPath = await FailureDumpWriter.TryWriteFailureDumpAsync(
                _store.Paths.DumpsDir,
                failed,
                ex,
                _store.GetRunLogs(runId),
                ct);

            if (dumpPath is not null)
                _store.AppendLog(runId, "INFO", $"Failure dump created: {dumpPath}");

            return failed;
        }
        finally
        {
            _store.ClearRunning();
            await _store.SaveStateAsync(ct);
        }
    }

    // ---------------- SYNC ----------------

    private async Task ExecuteSyncAsync(
        AbdsRunContext runCtx,
        AbdsJobRequest job,
        AbdsConfig cfg,
        CancellationToken ct)
    {
        var strategy = FileComparisonFactory.Create(
            cfg.Schedule.SyncComparisonMode,
            cfg.Schedule.HashBelowSizeMbThreshold,
            _hashCache);

        foreach (var target in job.Targets ?? Array.Empty<string>())
        {
            var plan = SyncPlanner.BuildPlan(job.SourcePath, target, strategy);

            long totalBytes = plan.Sum(p => p.Bytes);
            runCtx.SetTotals(totalBytes);
            runCtx.Commit();

            long copied = 0;
            var skipped = new List<string>();

            _store.AppendLog(runCtx.RunId, "INFO",
                $"Sync plan: {plan.Count} files, {FormatBytes(totalBytes)}");

            foreach (var item in plan)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    await ABDS.Core.IO.FileCopyWithRetry.CopyFileAtomicAsync(
                        item.SourceFile,
                        item.TargetFile,
                        ct,
                        m => Log(runCtx, "INFO", m),
                        m => Log(runCtx, "WARN", m),
                        m => Log(runCtx, "ERROR", m));

                    copied += item.Bytes;
                    runCtx.SetProgress(copied);
                    runCtx.Commit();
                }
                catch (Exception ex)
                {
                    skipped.Add(item.SourceFile);
                    await Log(runCtx, "WARN", $"Skipped: {item.SourceFile} ({ex.Message})");
                }
            }

            if (skipped.Count > 0)
            {
                runCtx.MarkPartiallyDone($"Skipped {skipped.Count} file(s).");
                foreach (var s in skipped)
                    runCtx.AddSkipped(s);
            }
        }

        await _hashCache.SaveAsync(ct);
    }

    // ---------------- BACKUP ----------------

    private async Task ExecuteBackupAsync(
        AbdsRunContext runCtx,
        AbdsJobRequest job,
        AbdsConfig cfg,
        CancellationToken ct)
    {
        var totalBytes = BackupEngine.CalculateTotalBytes(job.SourcePath);
        runCtx.SetTotals(totalBytes);
        runCtx.Commit();

        long copied = 0;

        await BackupEngine.RunBackupAsync(
            job.SourcePath,
            job.BackupRoot!,
            cfg.Schedule.MaxBackupStorageBytes,
            ct,
            m => Log(runCtx, "INFO", m),
            m => Log(runCtx, "WARN", m),
            m => Log(runCtx, "ERROR", m),
            async bytes =>
            {
                copied += bytes;
                runCtx.SetProgress(copied);
                runCtx.Commit();
                await Task.CompletedTask;
            });
    }

    // ---------------- HELPERS ----------------

    private AbdsRunContext FinalizeSuccess(AbdsRunContext ctx)
    {
        ctx.Complete("Success", "Completed successfully.");
        ctx.Commit();
        return ctx;
    }

    private Task Log(AbdsRunContext ctx, string level, string msg)
    {
        _store.AppendLog(ctx.RunId, level, msg);
        return Task.CompletedTask;
    }

    private static string FormatBytes(long b)
    {
        string[] u = ["B", "KB", "MB", "GB", "TB"];
        double v = b;
        int i = 0;
        while (v >= 1024 && i < u.Length - 1)
        {
            v /= 1024;
            i++;
        }
        return $"{v:0.##} {u[i]}";
    }
}
