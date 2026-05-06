using System.Collections.Concurrent;
using ABDS.Core.Destinations;
using ABDS.Core.Models;
using ABDS.SharedIpc;

namespace ABDS.Service;

public sealed class AbdsWorkerFacade(AbdsStateStore store, AbdsRunner runner, ILogger<AbdsWorkerFacade> log)
{
    private readonly ConcurrentQueue<AbdsJobRequest> _queue = new();
    private readonly ConcurrentQueue<AbdsJobRequest> _deferredJobs = new();
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _runCts = new();
    private readonly ConcurrentDictionary<string, byte> _deferredKeys = new();

    public bool HasDeferredJobs => !_deferredJobs.IsEmpty;

    public async Task<string> EnqueueForceSyncAllAsync(CancellationToken ct)
    {
        var cfg = await store.LoadConfigAsync(ct);
        var id = await ScheduleAsync(cfg.SyncPairs.Where(p => p.Enabled)
            .SelectMany(p => p.TargetPaths.Select(t => AbdsJobRequest.Sync(p.SourcePath, new[] { t }, "force-all"))), ct);

        return id ?? Guid.NewGuid().ToString("N");
    }

    public async Task<string> EnqueueForceBackupAllAsync(CancellationToken ct)
    {
        var cfg = await store.LoadConfigAsync(ct);
        var jobs = cfg.BackupSources.Where(b => b.Enabled)
            .Select(b => AbdsJobRequest.Backup(b.SourcePath, b.BackupRootPath, "force-all"));

        var id = await ScheduleAsync(jobs, ct);
        return id ?? Guid.NewGuid().ToString("N");
    }

    public Task<string> EnqueueForceSyncPairAsync(string src, string dst, CancellationToken ct)
        => EnqueueSingleAsync(AbdsJobRequest.Sync(src, new[] { dst }, "force-pair"), ct);

    public Task<string> EnqueueForceBackupSourceAsync(string src, string backupRoot, CancellationToken ct)
        => EnqueueSingleAsync(AbdsJobRequest.Backup(src, backupRoot, "force-source"), ct);

    public async Task<string> EnqueueScheduledJobAsync(AbdsJobRequest job, CancellationToken ct)
    {
        var queuedJob = job.WithRunId(Guid.NewGuid().ToString("N"));
        _queue.Enqueue(queuedJob);
        _ = Task.Run(() => TryRunNextAsync(ct), ct);

        await Task.CompletedTask;
        return queuedJob.RequestedRunId!;
    }

    public void Cancel(string runId)
    {
        if (_runCts.TryGetValue(runId, out var cts))
            cts.Cancel();
    }

    private async Task<string> EnqueueSingleAsync(AbdsJobRequest job, CancellationToken ct)
    {
        var queuedJob = job.WithRunId(Guid.NewGuid().ToString("N"));
        _queue.Enqueue(queuedJob);
        _ = Task.Run(() => TryRunNextAsync(ct), ct);

        await Task.CompletedTask;
        return queuedJob.RequestedRunId!;
    }

    private async Task<string?> ScheduleAsync(IEnumerable<AbdsJobRequest> jobs, CancellationToken ct)
    {
        string? firstRunId = null;

        foreach (var job in jobs)
        {
            var queuedJob = job.WithRunId(Guid.NewGuid().ToString("N"));
            firstRunId ??= queuedJob.RequestedRunId;
            _queue.Enqueue(queuedJob);
        }

        _ = Task.Run(() => TryRunNextAsync(ct), ct);
        await Task.CompletedTask;
        return firstRunId;
    }

    public async Task TryRunNextAsync(CancellationToken ct)
    {
        if (store.HasRunningJob())
            return;

        if (!_queue.TryDequeue(out var job))
            return;

        var cfg = await store.LoadConfigAsync(ct);

        var destinationProbe = await ProbeJobDestinationAsync(job, ct);
        if (destinationProbe is not null)
        {
            store.RecordDestinationProbe(destinationProbe);
            if (!destinationProbe.Available || !destinationProbe.Writable)
            {
                Defer(job);
                log.LogWarning(
                    "ABDS job deferred. Destination unavailable. Type={Type}, Destination={Destination}, Error={Error}",
                    job.Type,
                    destinationProbe.Location,
                    destinationProbe.ErrorMessage ?? destinationProbe.Status);

                store.UpdateTrayState(cfg);
                await store.SaveStateAsync(ct);

                if (!_queue.IsEmpty)
                    _ = Task.Run(() => TryRunNextAsync(ct), ct);

                return;
            }
        }

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        if (!string.IsNullOrWhiteSpace(job.RequestedRunId))
            _runCts[job.RequestedRunId] = cts;

        AbdsRunDetailsDto? run = null;
        try
        {
            run = await runner.RunJobAsync(job, cfg, cts.Token);
        }
        finally
        {
            if (!string.IsNullOrWhiteSpace(job.RequestedRunId))
                _runCts.TryRemove(job.RequestedRunId, out _);
        }

        log.LogInformation("ABDS job finished. RunId={RunId}, State={State}", run?.RunId, run?.State);

        store.UpdateTrayState(cfg);
        await store.SaveStateAsync(ct);

        if (!_queue.IsEmpty)
            _ = Task.Run(() => TryRunNextAsync(ct), ct);
    }

    public async Task RetryDeferredJobsAsync(CancellationToken ct)
    {
        var count = _deferredJobs.Count;
        for (var i = 0; i < count; i++)
        {
            if (!_deferredJobs.TryDequeue(out var job))
                break;

            _deferredKeys.TryRemove(JobKey(job), out _);
            _queue.Enqueue(job);
        }

        if (!_queue.IsEmpty)
        {
            await Task.CompletedTask;
            _ = Task.Run(() => TryRunNextAsync(ct), ct);
        }
    }

    private async Task<DestinationProbeResult?> ProbeJobDestinationAsync(AbdsJobRequest job, CancellationToken ct)
    {
        var destination = job.Type == AbdsTaskType.Sync
            ? job.Targets?.FirstOrDefault()
            : job.BackupRoot;

        if (string.IsNullOrWhiteSpace(destination))
            return null;

        return await DestinationProbe.ProbeAsync(destination, writeTest: true, ct);
    }

    private void Defer(AbdsJobRequest job)
    {
        var key = JobKey(job);
        if (_deferredKeys.TryAdd(key, 0))
            _deferredJobs.Enqueue(job);
    }

    private static string JobKey(AbdsJobRequest job)
        => $"{job.Type}|{job.SourcePath}|{string.Join(";", job.Targets ?? [])}|{job.BackupRoot}|{job.RequestedRunId}";
}
