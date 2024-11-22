using System.Threading.Tasks;

using TheKrystalShip.KGSM.Lib;
using TheKrystalShip.Logging;

namespace TheKrystalShip.KGSM.Services;

public class KgsmEventListener
{
    private readonly KgsmInterop _interop;
    private readonly WatchdogNotifier _watchdogNotifier;
    private readonly DiscordChannelRegistry _discordChannelRegistry;
    private readonly DiscordNotifier _discordNotifier;
    private readonly AppSettingsManager _settingsManager;
    private readonly Logger<KgsmEventListener> _logger;

    public KgsmEventListener(
            WatchdogNotifier watchdogNotifier,
            DiscordChannelRegistry discordChannelRegistry,
            AppSettingsManager settingsManager,
            DiscordNotifier discordNotifier,
            KgsmInterop interop
        )
    {
        _watchdogNotifier = watchdogNotifier;
        _discordChannelRegistry = discordChannelRegistry;
        _settingsManager = settingsManager;
        _discordNotifier = discordNotifier;
        _interop = interop;
        _logger = new();
    }

    public void Initialize()
    {
        _interop.Events.RegisterHandler<InstanceInstalledData>(OnInstanceInstalledAsync);
        _interop.Events.RegisterHandler<InstanceStartedData>(OnInstanceStartedAsync);
        _interop.Events.RegisterHandler<InstanceStoppedData>(OnInstanceStoppedAsync);
        _interop.Events.RegisterHandler<InstanceUninstalledData>(OnInstanceUninstalledAsync);

        _logger.LogInformation($"Registered event handlers");
    }

    private async Task OnInstanceInstalledAsync(InstanceInstalledData installedData)
    {
        await _discordChannelRegistry.AddOrUpdateChannelAsync(
            _settingsManager.Discord.Guild,
            installedData.Blueprint,
            installedData.InstanceId
        );
    }

    private async Task OnInstanceStartedAsync(InstanceStartedData startedData)
    {
        _watchdogNotifier.StartMonitoring(startedData.InstanceId);
        await Task.CompletedTask;
    }

    private async Task OnInstanceStoppedAsync(InstanceStoppedData stoppedData)
    {
        await _discordNotifier.OnRunningStatusUpdated(stoppedData.InstanceId, RunningStatus.Offline);
    }

    private async Task OnInstanceUninstalledAsync(InstanceUninstalledData uninstalledData)
    {
        await _discordChannelRegistry.RemoveChannelAsync(_settingsManager.Discord.Guild, uninstalledData.InstanceId);
    }
}
