using System.IO.Pipes;
using System.Text;
using System.Text.Json;

namespace ABDS.SharedIpc;

public sealed class AbdsIpcClient
{
    public async Task<AbdsCommandResponse> SendAsync(AbdsCommand cmd, CancellationToken ct = default)
    {
        using var pipe = new NamedPipeClientStream(".", AbdsIpc.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await pipe.ConnectAsync(1500, ct);

        var json = JsonSerializer.Serialize(cmd);
        var bytes = Encoding.UTF8.GetBytes(json + "\n");
        await pipe.WriteAsync(bytes, 0, bytes.Length, ct);
        await pipe.FlushAsync(ct);

        using var sr = new StreamReader(pipe, Encoding.UTF8, leaveOpen: true);
        var line = await sr.ReadLineAsync(ct);
        if (line is null)
            return new AbdsCommandResponse(false, "No response from service.");
        return JsonSerializer.Deserialize<AbdsCommandResponse>(line)!;
    }
}