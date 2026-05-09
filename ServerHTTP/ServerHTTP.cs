using Network.Handlers;
using Network.HttpMessages;
using Network.Routing;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerHTTP;

public class ServerHTTP
{
    private CancellationTokenSource _cts = new();
    private IPEndPoint _ipEndPoint;
    private IHandler _handler;

    public event Action<string>? OnLogging;

    public ServerHTTP(IPEndPoint ipEndpoint, IHandler handler)
    {
        _ipEndPoint = ipEndpoint;
        _handler = handler;
    }

    public ServerHTTP(IPAddress ipAddress, int port, IHandler handler)
        : this(IPEndPoint.Parse(ipAddress.ToString() + ":" + port), handler) { }

    public ServerHTTP(string ip, int port, IHandler handler)
        : this(IPAddress.Parse(ip), port, handler) { }

    public async Task ListenAsync()
    {
        TcpListener listener = new(_ipEndPoint);

        try
        {
            listener.Start();
            OnLogging?.Invoke("Listening on " + _ipEndPoint.ToString());
        }
        catch (SocketException ex)
        {
            OnLogging?.Invoke("An error occured while starting server: " + ex.Message);
            return;
        }

        try
        {
            while (!_cts.IsCancellationRequested)
            {
                TcpClient client = await listener.AcceptTcpClientAsync(_cts.Token);
                OnLogging?.Invoke($"{client.Client.RemoteEndPoint} is connected to server");
                _ = HandleClientAsync(client);
            }
        }
        catch (OperationCanceledException)
        {
            listener.Stop();
            return;
        }
        catch (SocketException ex)
        {
            OnLogging?.Invoke("An error occured while listening for clients: " + ex.Message);
            return;
        }
    }

    public void Stop()
    {
        OnLogging?.Invoke("Stopping server");
        _cts.Cancel();
        _cts = new();
    }

    private async Task HandleClientAsync(TcpClient tcpClient)
    {
        try
        {
            NetworkStream stream = tcpClient.GetStream();
            byte[] requestBytes = await GetRequestBytesAsync(stream);
            if (!Request.TryParse(Encoding.UTF8.GetString(requestBytes), out Request? request))
            {
                OnLogging?.Invoke($"{tcpClient.Client.RemoteEndPoint} sent an invalid request");
                await stream.WriteAsync(
                    new Response.Builder()
                        .SetStatusCode(400)
                        .SetBody(Encoding.UTF8.GetBytes(@"{""message"": ""Unable to parse request""}"))
                        .Build()
                        .GetMessage(),
                    _cts.Token
                );
                tcpClient.Close();
                return;
            }

            Response response;
            try
            {
                response = await _handler.Handle(new RequestContext(request!, "/"));
            }
            catch (Exception ex)
            {
                OnLogging?.Invoke($"An error occured while handling {tcpClient.Client.RemoteEndPoint} request: {ex.Message}");
                await stream.WriteAsync(
                    new Response.Builder()
                        .SetStatusCode(500)
                        .SetBody(Encoding.UTF8.GetBytes(@"{""message"": ""An error has occured""}"))
                        .Build()
                        .GetMessage(),
                    _cts.Token
                );
                return;
            }
            OnLogging?.Invoke($"Sending the handler's response to {tcpClient.Client.RemoteEndPoint}");
            await stream.WriteAsync(response.GetMessage(), _cts.Token);
            stream.Close();
        }
        catch (OperationCanceledException)
        {
            OnLogging?.Invoke($"Closing socket with {tcpClient.Client.RemoteEndPoint}");
            tcpClient.Close();
            return;
        }
        catch (IOException ex) when (ex.InnerException is SocketException se)
        {
            OnLogging?.Invoke($"Client {tcpClient.Client.RemoteEndPoint} generated an error: {se.Message}");
            return;
        }
        catch (SocketException ex)
        {
            OnLogging?.Invoke($"Client {tcpClient.Client.RemoteEndPoint} generated an error: {ex.Message}");
            return;
        }
    }

    private async Task<byte[]> GetRequestBytesAsync(NetworkStream stream)
    {
        using MemoryStream memStream = new();
        byte[] lineBuffer = new byte[4096];
        int currentLineBufferIndex = -1;

        int contentLength = 0;
        byte currentChar;
        while (!_cts.IsCancellationRequested)
        {
            currentChar = (byte)stream.ReadByte();
            lineBuffer[++currentLineBufferIndex] = currentChar;

            if (currentLineBufferIndex == 0)
                continue;

            byte lastChar = lineBuffer[currentLineBufferIndex - 1];
            if (lastChar == '\r' && currentChar == '\n')
            {
                await memStream.WriteAsync(lineBuffer.AsMemory(0, currentLineBufferIndex + 1), _cts.Token);
                currentLineBufferIndex = -1;
                lineBuffer = new byte[4096];
                byte[] memData = memStream.ToArray();
                if (memData.Length >= 4)
                {
                    byte[] lastFourBytes = memData[^4..];
                    if (lastFourBytes[0] == '\r'
                     && lastFourBytes[1] == '\n'
                     && lastFourBytes[2] == '\r'
                     && lastFourBytes[3] == '\n')
                        break;
                }
                string line = Encoding.UTF8.GetString(lineBuffer).Trim();
                string[] lineParts = line.Split(":");
                if (lineParts.Length < 2)
                    continue;
                if (lineParts[0].Trim().ToLower() != "content-length")
                    continue;
                _ = int.TryParse(lineParts[1].Trim(), out contentLength);
            }
        }

        if (contentLength > 0)
        {
            byte[] bodyContent = new byte[contentLength];
            await stream.ReadExactlyAsync(bodyContent, _cts.Token);

            await memStream.WriteAsync(bodyContent, _cts.Token);
        }
        return memStream.ToArray();
    }
}
