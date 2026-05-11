using Network.Handlers;
using Network.HttpMessages;
using Network.Routing;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerHTTP;

/// <summary>
/// Accepts incoming TCP clients, parses requests, and dispatches them through the configured handler.
/// </summary>
public class ServerHTTP
{
    private CancellationTokenSource _cts = new();
    private IPEndPoint _ipEndPoint;
    private IHandler _handler;

    /// <summary>
    /// Raised when the server wants to emit a log message.
    /// </summary>
    public event Action<string>? OnLogging;

    /// <summary>
    /// Initializes a new server instance for the specified endpoint and request handler.
    /// </summary>
    public ServerHTTP(IPEndPoint ipEndpoint, IHandler handler)
    {
        _ipEndPoint = ipEndpoint;
        _handler = handler;
    }

    /// <summary>
    /// Initializes a new server instance for the specified IP address, port, and request handler.
    /// </summary>
    public ServerHTTP(IPAddress ipAddress, int port, IHandler handler)
        : this(IPEndPoint.Parse(ipAddress.ToString() + ":" + port), handler) { }

    /// <summary>
    /// Initializes a new server instance for the specified IP string, port, and request handler.
    /// </summary>
    public ServerHTTP(string ip, int port, IHandler handler)
        : this(IPAddress.Parse(ip), port, handler) { }

    /// <summary>
    /// Starts listening for incoming client connections and processes each request asynchronously.
    /// </summary>
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

    /// <summary>
    /// Stops the server and cancels any pending accept or request operations.
    /// </summary>
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
        byte[] buffer = new byte[4096];
        int contentLength = 0;

        while (!_cts.IsCancellationRequested)
        {
            int bytesRead = await stream.ReadAsync(buffer, _cts.Token);
            if (bytesRead == 0) break;

            await memStream.WriteAsync(buffer.AsMemory(0, bytesRead), _cts.Token);

            byte[] data = memStream.ToArray();
            int headerEnd = -1;

            for (int i = 0; i <= data.Length - 4; i++)
            {
                if (data[i] == '\r' && data[i + 1] == '\n' && data[i + 2] == '\r' && data[i + 3] == '\n')
                {
                    headerEnd = i;
                    break;
                }
            }

            if (headerEnd == -1) continue;

            string headers = Encoding.UTF8.GetString(data, 0, headerEnd);
            foreach (string line in headers.Split("\r\n"))
            {
                int colonIndex = line.IndexOf(':');
                if (colonIndex <= 0) continue;
                if (line[..colonIndex].Trim().ToLower() == "content-length")
                    int.TryParse(line[(colonIndex + 1)..].Trim(), out contentLength);
            }

            int headerTotalLength = headerEnd + 4;
            byte[] alreadyReadBody = data[headerTotalLength..];
            int remaining = contentLength - alreadyReadBody.Length;

            if (remaining > 0)
            {
                byte[] restOfBody = new byte[remaining];
                await stream.ReadExactlyAsync(restOfBody, _cts.Token);
                await memStream.WriteAsync(alreadyReadBody, _cts.Token);
                await memStream.WriteAsync(restOfBody, _cts.Token);
            }

            break;
        }

        return memStream.ToArray();
    }
}
