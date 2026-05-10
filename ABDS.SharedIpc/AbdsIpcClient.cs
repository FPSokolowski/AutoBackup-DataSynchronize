using System.IO.Pipes;
using System.Text;
using System.Text.Json;

namespace ABDS.SharedIpc;

public sealed class AbdsIpcClient
{
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(4);

    public async Task<AbdsCommandResponse> SendAsync(AbdsCommand cmd, CancellationToken ct = default)
    {
        Exception? lastError = null;
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                return await SendOnceAsync(cmd, ct);
            }
            catch (OperationCanceledException ex) when (!ct.IsCancellationRequested)
            {
                lastError = new TimeoutException("Timed out waiting for ABDS service response.", ex);
            }
            catch (Exception ex) when (ex is TimeoutException or IOException)
            {
                lastError = ex;
            }

            if (attempt < 3)
                await Task.Delay(TimeSpan.FromMilliseconds(150 * attempt), ct);
        }

        throw lastError ?? new TimeoutException("Timed out waiting for ABDS service response.");
    }

    private static async Task<AbdsCommandResponse> SendOnceAsync(AbdsCommand cmd, CancellationToken ct)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(RequestTimeout);
        var requestCt = timeout.Token;

        using var pipe = new NamedPipeClientStream(".", AbdsIpc.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await pipe.ConnectAsync(1500, requestCt);

        var json = JsonSerializer.Serialize(cmd);
        var bytes = Encoding.UTF8.GetBytes(json + "\n");
        await pipe.WriteAsync(bytes, 0, bytes.Length, requestCt);
        await pipe.FlushAsync(requestCt);

        using var sr = new StreamReader(pipe, Encoding.UTF8, leaveOpen: true);
        var line = await sr.ReadLineAsync(requestCt);
        if (line is null)
            return new AbdsCommandResponse(false, "No response from service.");
        return JsonSerializer.Deserialize<AbdsCommandResponse>(line)!;
    }
}
