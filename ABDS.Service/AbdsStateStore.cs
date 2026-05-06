using System.Text.Json;
using ABDS.Core.Models;
using ABDS.SharedIpc;

namespace ABDS.Service;

public sealed class AbdsStateStore
{
    private readonly object _lock = new();

    public AbdsPaths Paths { get; } = AbdsPaths.Default();

    private string? _runningRunId;

    private AbdsServiceState _state = new();
    private AbdsConfig? _config;

    private readonly Dictionary<string, List<AbdsRunLogLineDto>> _runLogs = new();
    private readonly Dictionary<string, AbdsRunDetailsDto> _runs = new();

    public bool HasRunningJob()
    {
        lock (_lock)
            return _runningRunId is not null;
    }

    public void SetRunning(string runId)
    {
        lock (_lock)
            _runningRunId = runId;
    }

    public void ClearRunning()
    {
        lock (_lock)
            _runningRunId = null;
    }

    public AbdsRunContext CreateRun(string runId, AbdsJobRequest job)
    {
        var details = new AbdsRunDetailsDto(
            RunId: runId,
            TaskType: job.Type.ToString(),
            State: "Running",
            StartedAt: DateTimeOffset.Now,
            FinishedAt: null,
            Summary: null,
            TotalBytes: 0,
            CopiedBytes: 0,
            Sources: new List<string> { job.SourcePath },
            Targets: job.Targets?.ToList() ?? (job.BackupRoot is null ? new List<string>() : new List<string> { job.BackupRoot }),
            PartiallySkippedFiles: new List<string>(),
            Errors: new List<string>()
        );

        lock (_lock)
        {
            _runs[runId] = details;
            _runLogs[runId] = new List<AbdsRunLogLineDto>();
        }

        return new AbdsRunContext(this, details);
    }

    public void UpsertRun(AbdsRunDetailsDto run)
    {
        lock (_lock)
        {
            _runs[run.RunId] = run;
            _state.LastRuns[run.RunId] = run;
        }

        UpdatePerPathStatuses(run);
    }

    public void AppendLog(string runId, string level, string message)
    {
        var line = new AbdsRunLogLineDto(DateTimeOffset.Now, level, message);
        lock (_lock)
        {
            if (!_runLogs.TryGetValue(runId, out var list))
                _runLogs[runId] = list = new();

            list.Add(line);
            if (list.Count > 10_000)
                list.RemoveRange(0, 2_000); // guard
        }
    }

    public List<AbdsRunLogLineDto> GetRunLogs(string runId)
    {
        lock (_lock)
            return _runLogs.TryGetValue(runId, out var list)
                ? list.ToList()
                : new List<AbdsRunLogLineDto>();
    }

    public AbdsRunDetailsDto? GetRun(string runId)
    {
        lock (_lock)
            return _runs.TryGetValue(runId, out var r) ? r : null;
    }

    public List<AbdsRunDetailsDto> GetRecentRuns(int take)
    {
        lock (_lock)
        {
            return _state.LastRuns.Values
                .Concat(_runs.Values)
                .GroupBy(r => r.RunId, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.OrderByDescending(r => r.FinishedAt ?? r.StartedAt).First())
                .OrderByDescending(r => r.FinishedAt ?? r.StartedAt)
                .Take(Math.Clamp(take, 1, 200))
                .ToList();
        }
    }

    public async Task<AbdsConfig> LoadConfigAsync(CancellationToken ct)
    {
        if (_config is not null)
            return _config;

        Directory.CreateDirectory(Paths.RootDir);

        if (!File.Exists(Paths.ConfigPath))
        {
            _config = new AbdsConfig(); // default
            await SaveConfigAsync(_config, ct);
            return _config;
        }

        var json = await File.ReadAllTextAsync(Paths.ConfigPath, ct);
        _config = JsonSerializer.Deserialize<AbdsConfig>(json) ?? new AbdsConfig();
        return _config;
    }

    public async Task SaveConfigAsync(AbdsConfig cfg, CancellationToken ct)
    {
        Directory.CreateDirectory(Paths.RootDir);
        var json = JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(Paths.ConfigPath, json, ct);
        _config = cfg;
    }

    public async Task LoadStateAsync(CancellationToken ct)
    {
        Directory.CreateDirectory(Paths.RootDir);

        if (!File.Exists(Paths.StatePath))
        {
            _state = new AbdsServiceState();
            await SaveStateAsync(ct);
            return;
        }

        var json = await File.ReadAllTextAsync(Paths.StatePath, ct);
        _state = JsonSerializer.Deserialize<AbdsServiceState>(json) ?? new AbdsServiceState();
    }

    public async Task SaveStateAsync(CancellationToken ct)
    {
        Directory.CreateDirectory(Paths.RootDir);
        var json = JsonSerializer.Serialize(_state, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(Paths.StatePath, json, ct);
    }

    // ---- Status snapshot for GUI ----
    public AbdsStatusSnapshotDto BuildStatusSnapshot(AbdsConfig cfg)
    {
        lock (_lock)
        {
            return new AbdsStatusSnapshotDto(
                TraySeverity: _state.TraySeverity,
                TrayTooltip: _state.TrayTooltip,
                HasRunningJob: _runningRunId is not null,
                RunningRunId: _runningRunId,
                SyncStatuses: _state.SyncStatuses.Values.ToList(),
                BackupStatuses: _state.BackupStatuses.Values.ToList(),
                DestinationStatuses: _state.DestinationStatuses.Values
                    .Select(d => new AbdsDestinationStatusDto(
                        d.Location,
                        d.Kind.ToString(),
                        d.Available,
                        d.Writable,
                        d.Status,
                        d.ErrorMessage,
                        d.TestedAt))
                    .ToList()
            );
        }
    }

    public void RecordDestinationProbe(DestinationProbeResult result)
    {
        lock (_lock)
            _state.DestinationStatuses[result.Location] = result;
    }

    // ---- Tray severity logic (Busy/Ok/Warning/Critical) ----
    public void UpdateTrayState(AbdsConfig cfg)
    {
        lock (_lock)
        {
            if (_runningRunId is not null)
            {
                _state.TraySeverity = "Busy";
                _state.TrayTooltip = "ABDS: wykonywanie zadania...";
                return;
            }

            var now = DateTimeOffset.Now;

            var issues = new List<string>();
            var critical = false;
            var warning = false;

            foreach (var d in _state.DestinationStatuses.Values)
            {
                if (!d.Available || !d.Writable)
                {
                    warning = true;
                    issues.Add($"DESTINATION WARNING: {d.Location} ({d.ErrorMessage ?? d.Status})");
                }
            }

            // Sync overdue rules
            if (cfg.Schedule.AutoSyncEnabled)
            {
                foreach (var s in _state.SyncStatuses.Values)
                {
                    var lastOk = s.LastSuccessAt;
                    var interval = cfg.Schedule.AutoSyncInterval;

                    if (lastOk is null)
                    {
                        warning = true;
                        issues.Add($"SYNC: brak udanej synchronizacji: {s.SourcePath} -> {s.TargetPath}");
                        continue;
                    }

                    var overdue = now - lastOk.Value;
                    if (overdue > interval * cfg.CriticalSyncOverdueFactor)
                    {
                        critical = true;
                        issues.Add($"SYNC CRITICAL: {s.SourcePath} -> {s.TargetPath} (overdue {overdue})");
                    }
                    else if (overdue > interval)
                    {
                        warning = true;
                        issues.Add($"SYNC WARNING: {s.SourcePath} -> {s.TargetPath} (overdue {overdue})");
                    }

                    if (s.LastState is "Failed" or "PartiallyDone")
                    {
                        warning = true;
                        issues.Add($"SYNC {s.LastState}: {s.SourcePath} -> {s.TargetPath} ({s.LastErrorMessage})");
                    }
                }
            }

            // Backup overdue rules
            if (cfg.Schedule.AutoBackupEnabled)
            {
                foreach (var b in _state.BackupStatuses.Values)
                {
                    var lastOk = b.LastSuccessAt;
                    var interval = cfg.Schedule.AutoBackupIntervalFromLastSuccess;

                    if (lastOk is null)
                    {
                        warning = true;
                        issues.Add($"BACKUP: brak udanego backupu: {b.SourcePath}");
                        continue;
                    }

                    var overdue = now - lastOk.Value;
                    var criticalThreshold = interval + cfg.CriticalBackupOverdueExtra;

                    if (overdue > criticalThreshold)
                    {
                        critical = true;
                        issues.Add($"BACKUP CRITICAL: {b.SourcePath} (overdue {overdue})");
                    }
                    else if (overdue > interval)
                    {
                        warning = true;
                        issues.Add($"BACKUP WARNING: {b.SourcePath} (overdue {overdue})");
                    }

                    if (b.LastState is "Failed")
                    {
                        warning = true;
                        issues.Add($"BACKUP FAILED: {b.SourcePath} ({b.LastErrorMessage})");
                    }
                }
            }

            _state.TraySeverity = critical ? "Critical" : warning ? "Warning" : "Ok";
            _state.TrayTooltip = issues.Count == 0
                ? "ABDS: OK"
                : "ABDS:\n" + string.Join("\n", issues.Take(10));
            if (_state.TraySeverity == "Critical")
            {
                _state.CriticalTicksInRow++;
                var canToast = !_state.LastCriticalToastAt.HasValue || (now - _state.LastCriticalToastAt.Value) > TimeSpan.FromHours(2);

#if WINDOWS
    if (_state.CriticalTicksInRow >= 3 && canToast)
    {
        WindowsToastNotifier.ShowCritical("ABDS: Krytycznie nieaktualny backup/sync", _state.TrayTooltip);
        _state.LastCriticalToastAt = now;
    }
#endif
            }
            else
            {
                _state.CriticalTicksInRow = 0;
            }
        }
    }

    // ---- Retry / scheduling decision ----
    public AbdsJobRequest? DecideNextJob(AbdsConfig cfg, DateTimeOffset now)
    {
        lock (_lock)
        {
            if (_runningRunId is not null)
                return null;

            // 1) retry partiallyDone sync after >10 min
            foreach (var s in _state.SyncStatuses.Values)
            {
                if (s.LastState == "PartiallyDone" && s.LastAttemptAt.HasValue)
                {
                    if (now - s.LastAttemptAt.Value > cfg.RetryPartialSyncAfter)
                        return AbdsJobRequest.Sync(s.SourcePath, new[] { s.TargetPath }, reason: "retry-partiallyDone");
                }
            }

            // 2) retry failed sync after >10 min
            foreach (var s in _state.SyncStatuses.Values)
            {
                if (s.LastState == "Failed" && s.LastAttemptAt.HasValue)
                {
                    if (now - s.LastAttemptAt.Value > cfg.RetryFailedSyncAfter)
                        return AbdsJobRequest.Sync(s.SourcePath, new[] { s.TargetPath }, reason: "retry-failed");
                }
            }

            // 3) retry failed backup after >30 min
            foreach (var b in _state.BackupStatuses.Values)
            {
                if (b.LastState == "Failed" && b.LastAttemptAt.HasValue)
                {
                    if (now - b.LastAttemptAt.Value > cfg.RetryFailedBackupAfter)
                        return AbdsJobRequest.Backup(b.SourcePath, b.BackupRootPath, reason: "retry-failed");
                }
            }

            // 4) normal scheduled runs (interval-based)
            if (cfg.Schedule.AutoSyncEnabled)
            {
                foreach (var pair in cfg.SyncPairs.Where(p => p.Enabled))
                {
                    foreach (var target in pair.TargetPaths)
                    {
                        var key = AbdsServiceState.SyncKey(pair.SourcePath, target);
                        _state.SyncStatuses.TryGetValue(key, out var st);

                        if (st?.LastSuccessAt is null || ( now - st.LastSuccessAt.Value ) > cfg.Schedule.AutoSyncInterval)
                            return AbdsJobRequest.Sync(pair.SourcePath, new[] { target }, reason: "interval");
                    }
                }
            }

            if (cfg.Schedule.AutoBackupEnabled)
            {
                foreach (var src in cfg.BackupSources.Where(b => b.Enabled))
                {
                    var key = AbdsServiceState.BackupKey(src.SourcePath, src.BackupRootPath);
                    _state.BackupStatuses.TryGetValue(key, out var st);

                    if (st?.LastSuccessAt is null || ( now - st.LastSuccessAt.Value ) > cfg.Schedule.AutoBackupIntervalFromLastSuccess)
                        return AbdsJobRequest.Backup(src.SourcePath, src.BackupRootPath, reason: "interval");
                }
            }

            return null;
        }
    }

    // Per-path status update
    private void UpdatePerPathStatuses(AbdsRunDetailsDto run)
    {
        lock (_lock)
        {
            if (run.TaskType == "Sync" && run.Targets.Count > 0)
            {
                foreach (var t in run.Targets)
                {
                    var key = AbdsServiceState.SyncKey(run.Sources[0], t);
                    _state.SyncStatuses[key] = new AbdsPairStatusDto(
                        SourcePath: run.Sources[0],
                        TargetPath: t,
                        LastState: run.State,
                        LastAttemptAt: run.StartedAt,
                        LastSuccessAt: run.State == "Success" ? run.FinishedAt : _state.SyncStatuses.GetValueOrDefault(key)?.LastSuccessAt,
                        LastErrorCode: run.State == "Failed" ? "SYNC_FAILED" : null,
                        LastErrorMessage: run.State == "Failed" ? run.Summary : null
                    );
                }
            }

            if (run.TaskType == "Backup" && run.Targets.Count > 0)
            {
                var key = AbdsServiceState.BackupKey(run.Sources[0], run.Targets[0]);
                _state.BackupStatuses[key] = new AbdsBackupStatusDto(
                    SourcePath: run.Sources[0],
                    BackupRootPath: run.Targets[0],
                    LastState: run.State,
                    LastAttemptAt: run.StartedAt,
                    LastSuccessAt: run.State == "Success" ? run.FinishedAt : _state.BackupStatuses.GetValueOrDefault(key)?.LastSuccessAt,
                    LastErrorCode: run.State == "Failed" ? "BACKUP_FAILED" : null,
                    LastErrorMessage: run.State == "Failed" ? run.Summary : null
                );
            }
        }
    }
}

// ---- state persisted to state.json ----
public sealed class AbdsServiceState
{
    public string TraySeverity { get; set; } = "Ok";
    public string TrayTooltip { get; set; } = "ABDS: OK";

    public int CriticalTicksInRow { get; set; } = 0;
    public DateTimeOffset? LastCriticalToastAt { get; set; }

    public Dictionary<string, AbdsPairStatusDto> SyncStatuses { get; set; } = new();
    public Dictionary<string, AbdsBackupStatusDto> BackupStatuses { get; set; } = new();

    public Dictionary<string, AbdsRunDetailsDto> LastRuns { get; set; } = new(); // bounded in practice
    public Dictionary<string, DestinationProbeResult> DestinationStatuses { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public static string SyncKey(string src, string dst) => $"{src}|||{dst}";
    public static string BackupKey(string src, string root) => $"{src}|||{root}";
}

// ---- job request & run context ----
public sealed record AbdsJobRequest(
    AbdsTaskType Type,
    string SourcePath,
    string[]? Targets,
    string? BackupRoot,
    string Reason,
    string? RequestedRunId = null)
{
    public static AbdsJobRequest Sync(string src, IEnumerable<string> targets, string reason)
        => new(AbdsTaskType.Sync, src, targets.ToArray(), null, reason);

    public static AbdsJobRequest Backup(string src, string backupRoot, string reason)
        => new(AbdsTaskType.Backup, src, null, backupRoot, reason);

    public AbdsJobRequest WithRunId(string runId)
        => this with { RequestedRunId = runId };
}

public sealed class AbdsRunContext
{
    private readonly AbdsStateStore _store;
    private AbdsRunDetailsDto _run;

    public AbdsRunDetailsDto Run => _run;
    public string RunId => _run.RunId;

    public AbdsRunContext(AbdsStateStore store, AbdsRunDetailsDto run)
    {
        _store = store;
        _run = run;
    }

    public void SetTotals(long totalBytes)
        => _run = _run with { TotalBytes = totalBytes };

    public void SetProgress(long copiedBytes)
        => _run = _run with { CopiedBytes = copiedBytes };

    public void MarkPartiallyDone(string summary)
        => _run = _run with { State = "PartiallyDone", Summary = summary, FinishedAt = DateTimeOffset.Now };

    public void Complete(string state, string summary)
        => _run = _run with { State = state, Summary = summary, FinishedAt = DateTimeOffset.Now };

    public void AddSkipped(string path)
        => _run = _run with { PartiallySkippedFiles = _run.PartiallySkippedFiles.Concat(new[] { path }).ToList() };

    public void AddError(string err)
        => _run = _run with { Errors = _run.Errors.Concat(new[] { err }).ToList() };

    public AbdsRunDetailsDto ToDetails() => _run;

    public void Commit()
        => _store.UpsertRun(_run);
}
