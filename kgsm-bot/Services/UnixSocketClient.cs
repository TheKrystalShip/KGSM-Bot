using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TheKrystalShip.Logging;

namespace TheKrystalShip.KGSM.Services;

public class UnixSocketClient : IDisposable
{
    private string _socketPath = "";
    private readonly Logger<UnixSocketClient> _logger;
    public event Func<string, Task> EventReceived;

    public UnixSocketClient()
    {
        _logger = new();
    }

    ~UnixSocketClient()
    {
        this.Dispose();
    }

    public void Dispose()
    {
        if (File.Exists(_socketPath))
        {
            File.Delete(_socketPath);
        }
    }

    public async Task StartListeningAsync(string socketPath, CancellationToken token)
    {
        _socketPath = socketPath;

        try
        {
            if (File.Exists(socketPath))
            {
                File.Delete(socketPath);
            }

            using Socket server = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

            var endpoint = new UnixDomainSocketEndPoint(socketPath);
            server.Bind(endpoint);
            server.Listen(backlog: 5);

            _logger.LogInformation($"Connected to socket: {socketPath}");

            // Reader loop
            while (!token.IsCancellationRequested)
            {
                using Socket client = await server.AcceptAsync(token);
                using var networkStream = new NetworkStream(client);
            
                byte[] buffer = new byte[1024];
                int bytesRead;

                while ((bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                    
                    if (string.IsNullOrEmpty(message))
                        continue;
                    
                    this.EventReceived?.Invoke(message);
                }
            }
        
            _logger.LogError($"Cancellation requested, aborting");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error: {ex.Message}. Retrying in 5 seconds...");
            await Task.Delay(5000, token);
        }
    }
}

