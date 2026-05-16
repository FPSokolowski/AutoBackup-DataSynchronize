using Grpc.Core;
using Grpc.Net.Client;

namespace ABDS.SharedIpc;

public sealed class AbdsIpcClient : IDisposable
{
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(4);
    private readonly GrpcChannel _channel;
    private readonly AbdsIpcGrpc.AbdsIpcGrpcClient _client;

    static AbdsIpcClient()
    {
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
    }

    public AbdsIpcClient()
        : this(AbdsIpc.GrpcEndpoint)
    {
    }

    public AbdsIpcClient(string endpoint)
    {
        _channel = GrpcChannel.ForAddress(endpoint);
        _client = new AbdsIpcGrpc.AbdsIpcGrpcClient(_channel);
    }

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
            catch (Exception ex) when (ex is TimeoutException or RpcException)
            {
                lastError = ex;
            }

            if (attempt < 3)
                await Task.Delay(TimeSpan.FromMilliseconds(150 * attempt), ct);
        }

        throw lastError ?? new TimeoutException("Timed out waiting for ABDS service response.");
    }

    private async Task<AbdsCommandResponse> SendOnceAsync(AbdsCommand cmd, CancellationToken ct)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(RequestTimeout);
        var requestCt = timeout.Token;

        var response = await _client.SendAsync(cmd.ToGrpc(), cancellationToken: requestCt);
        return response.ToContract();
    }

    public void Dispose()
    {
        _channel.Dispose();
    }
}
