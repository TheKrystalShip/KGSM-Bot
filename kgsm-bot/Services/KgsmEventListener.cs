using System;
using System.Text.Json;
using System.Threading.Tasks;

using TheKrystalShip.KGSM.Domain;

using TheKrystalShip.Logging;

namespace TheKrystalShip.KGSM.Services;

public class KgsmEventListener
{
    private readonly UnixSocketClient _client;
    private readonly WatchdogNotifier _watchdogNotifier;
    private readonly DiscordChannelRegistry _discordChannelRegistry;
    private readonly DiscordNotifier _discordNotifier;
    private readonly AppSettingsManager _settingsManager;
    private readonly Logger<KgsmEventListener> _logger;
    private readonly CancellationTokenSource _cts;

    private readonly Dictionary<string, Type> _eventTypeMapping = new()
    {
        { "instance_installed", typeof(InstanceInstalledData) },
        { "instance_started", typeof(InstanceStartedData) },
        { "instance_stopped", typeof(InstanceStoppedData) },
        { "instance_uninstalled", typeof(InstanceUninstalledData) }
    };

    public KgsmEventListener(WatchdogNotifier watchdogNotifier, DiscordChannelRegistry discordChannelRegistry, AppSettingsManager settingsManager, DiscordNotifier discordNotifier)
    {
        _watchdogNotifier = watchdogNotifier;
        _discordChannelRegistry = discordChannelRegistry;
        _settingsManager = settingsManager;
        _discordNotifier = discordNotifier;
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

    private EventDataBase DeserializeEventData(string eventType, JsonElement dataElement)
    {
        if (_eventTypeMapping.TryGetValue(eventType, out Type? targetType))
        {
            if (targetType == null)
                throw new InvalidOperationException($"Unknown event type {eventType}");

            var deserializationResult = JsonSerializer
                .Deserialize(dataElement.GetRawText(), targetType) as EventDataBase;

            if (deserializationResult == null)
                throw new InvalidOperationException($"Unknown event type {eventType}");

            return deserializationResult;
        }

        throw new InvalidOperationException($"Unknown event type: {eventType}");
    }

    private async Task OnEventReceivedAsync(string message)
    {
        _logger.LogInformation($"Received event: {message}");

        try
        {
            // Deserialize the outer wrapper
            var eventWrapper = JsonSerializer.Deserialize<EventWrapper>(message);

            if (eventWrapper == null || string.IsNullOrWhiteSpace(eventWrapper.EventType))
            {
                _logger.LogError("Invalid event received, missing EventType.");
                return;
            }

            // Deserialize the Data based on the EventType
            var eventData = DeserializeEventData(eventWrapper.EventType, eventWrapper.Data);

            switch (eventData)
            {
                case InstanceInstalledData installedData:
                    _logger.LogInformation($"Instance installed with ID: {installedData.InstanceId}, blueprint: {installedData.Blueprint}");
                    // Handle instance_created event
                    await _discordChannelRegistry.AddOrUpdateChannelAsync(
                            _settingsManager.Discord.GuildId,
                            installedData.Blueprint,
                            installedData.InstanceId
                        );
                    break;

                case InstanceStartedData startedData:
                    _logger.LogInformation($"Instance started with ID: {startedData.InstanceId}");
                    _watchdogNotifier.StartMonitoring(startedData.InstanceId);
                    break;

                case InstanceStoppedData stoppedData:
                    _logger.LogInformation($"Instance stopped with ID: {stoppedData.InstanceId}");
                    await _discordNotifier.OnRunningStatusUpdated(new(stoppedData.InstanceId, RunningStatus.Offline));
                    break;

                case InstanceUninstalledData uninstalledData:
                    _logger.LogInformation($"Instance uninstalled with ID: {uninstalledData.InstanceId}");
                    await _discordChannelRegistry.RemoveChannelAsync(_settingsManager.Discord.GuildId, uninstalledData.InstanceId);
                    break;

                default:
                    _logger.LogError($"Unhandled event type: {eventWrapper.EventType}");
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing event message.");
        }

        await Task.CompletedTask;
    }
}
