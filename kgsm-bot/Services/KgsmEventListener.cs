using System;
using System.Threading.Tasks;

using TheKrystalShip.Logging;

namespace TheKrystalShip.KGSM.Services;

public class KgsmEventListener
{
    private readonly UnixSocketClient _client;
    private readonly WatchdogNotifier _watchdogNotifier;
    private readonly Logger<KgsmEventListener> _logger;
    private readonly CancellationTokenSource _cts;

    public KgsmEventListener(WatchdogNotifier watchdogNotifier)
    {
        _watchdogNotifier = watchdogNotifier;
        _client = new();
        _logger = new();
        _cts = new();
    }

    ~KgsmEventListener()
    {
        _cts.Cancel();
    }

    public void Initialize(string socketPath)
    {
        _client.EventReceived += OnEventReceivedAsync;
        Task.Run(() => _client.StartListeningAsync(socketPath, _cts.Token));
    }

    public async Task OnEventReceivedAsync(string message)
    {
        _logger.LogInformation($"Event received: {message}");

        if (message.StartsWith("instance_started"))
        {
            string instanceId = message.Split(':')[1];
            _watchdogNotifier.StartMonitoring(instanceId);
        }
        else if (message.StartsWith("instance_stopped"))
        {
            string instanceId = message.Split(':')[1];
            _watchdogNotifier.StopMonitoring(instanceId);
        }

        await Task.CompletedTask;
    }
}
