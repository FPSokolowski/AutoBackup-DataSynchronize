using ABDS.Core.Hashing;
using ABDS.Core.Models;

namespace ABDS.Service;

public sealed class AbdsWorker(
    ILogger<AbdsWorker> log,
    AbdsStateStore store,
    AbdsIpcServer ipc,
    IHashCache hashCache,
    AbdsWorkerFacade facade
) : BackgroundService
{
    private DateTimeOffset _lastCacheSave = DateTimeOffset.MinValue;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // ---------- LOAD CACHE AT START ----------
        await hashCache.LoadAsync(stoppingToken);
        log.LogInformation("Hash cache loaded.");
        // -----------------------------------------

        await store.LoadStateAsync(stoppingToken);
        await ipc.StartAsync(stoppingToken);
        var nextDelay = TimeSpan.FromMinutes(1);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var cfg = await store.LoadConfigAsync(stoppingToken);

                // ---------- TRIM CACHE EVERY TICK ----------
                hashCache.TrimIfNeeded();
                // -------------------------------------------

                await TickAsync(cfg, stoppingToken);
                nextDelay = facade.HasDeferredJobs
                    ? TimeSpan.FromSeconds(60)
                    : TimeSpan.FromMinutes(Math.Max(1, cfg.ServiceTickMinutes));

                // ---------- PERIODIC CACHE SAVE (co 10 min) ----------
                if (DateTimeOffset.Now - _lastCacheSave > TimeSpan.FromMinutes(10))
                {
                    await hashCache.SaveAsync(stoppingToken);
                    _lastCacheSave = DateTimeOffset.Now;
                    log.LogInformation("Hash cache saved (periodic).");
                }
                // -----------------------------------------------------
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Worker tick failed.");
            }

            await Task.Delay(nextDelay, stoppingToken);
        }

        // ---------- SAVE CACHE ON SHUTDOWN ----------
        await hashCache.SaveAsync(stoppingToken);
        log.LogInformation("Hash cache saved (shutdown).");
        // -------------------------------------------
    }

    private async Task TickAsync(AbdsConfig cfg, CancellationToken ct)
    {
        if (store.HasRunningJob())
            return;

        if (facade.HasDeferredJobs)
        {
            await facade.RetryDeferredJobsAsync(ct);
            return;
        }

        var next = store.DecideNextJob(cfg, DateTimeOffset.Now);
        if (next is null)
        {
            store.UpdateTrayState(cfg);
            return;
        }

        var runId = await facade.EnqueueScheduledJobAsync(next, ct);
        log.LogInformation("Scheduled ABDS job queued. RunId={RunId}, Type={Type}, Reason={Reason}", runId, next.Type, next.Reason);

        // ---------- SAVE CACHE AFTER SYNC ----------
        if (next.Type == AbdsTaskType.Sync)
        {
            await hashCache.SaveAsync(ct);
            log.LogInformation("Hash cache saved (after sync).");
        }
        // -------------------------------------------
    }
}
