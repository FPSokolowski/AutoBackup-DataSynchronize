using ABDS.SharedIpc;

namespace ABDS.App.Services;

public sealed class IpcService
{
    private readonly AbdsIpcClient _client = new();

    public Task<AbdsCommandResponse> SendAsync(AbdsCommand cmd, CancellationToken ct = default)
        => _client.SendAsync(cmd, ct);

    public async Task<AbdsStatusSnapshotDto?> GetStatusAsync(CancellationToken ct = default)
    {
        var res = await _client.SendAsync(new AbdsCommand(AbdsCommandType.GetStatus), ct);
        if (!res.Ok)
            return null;

        // response.Message zawiera JSON snapshot
        return System.Text.Json.JsonSerializer.Deserialize<AbdsStatusSnapshotDto>(res.Message);
    }

    public async Task<AbdsRunDetailsDto?> GetRunDetailsAsync(string runId, CancellationToken ct = default)
    {
        var res = await _client.SendAsync(new AbdsCommand(AbdsCommandType.GetRunDetails, new() { ["runId"] = runId }), ct);
        if (!res.Ok)
            return null;
        return System.Text.Json.JsonSerializer.Deserialize<AbdsRunDetailsDto>(res.Message);
    }

    public async Task<List<AbdsRunLogLineDto>> GetRunLogsAsync(string runId, CancellationToken ct = default)
    {
        var res = await _client.SendAsync(new AbdsCommand(AbdsCommandType.GetRunLogs, new() { ["runId"] = runId }), ct);
        if (!res.Ok)
            return new();
        return System.Text.Json.JsonSerializer.Deserialize<List<AbdsRunLogLineDto>>(res.Message) ?? new();
    }
}