using System.Collections.Concurrent;
using System.Diagnostics;

using TheKrystalShip.Logging;

namespace TheKrystalShip.KGSM.Services;

public class WatchdogNotifier
{
    private readonly Logger<WatchdogNotifier> _logger;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _serviceThreads;
    private readonly DiscordNotifier _discordNotifier;
    private readonly AppSettingsManager _settingsManager;

    public WatchdogNotifier(DiscordNotifier discordNotifier, AppSettingsManager settingsManager)
    {
        _logger = new();
        _serviceThreads = new();
        _discordNotifier = discordNotifier;
        _settingsManager = settingsManager;
    }

    public void StartMonitoring(string instanceId)
    {
        if (_serviceThreads.ContainsKey(instanceId))
        {
            _logger.LogError($"Instance {instanceId} is already being monitored");
            return;
        }

        var trigger = _settingsManager.GetTrigger(instanceId);

        if (trigger == null)
        {
            _logger.LogError($"Triggers for instance: {instanceId} is null");
            return;
        }
        
        var cts = new CancellationTokenSource();
        _serviceThreads[instanceId] = cts;

        Task.Run(() => MonitorInstanceAsync(instanceId, trigger, cts.Token));
        _logger.LogInformation($"Started monitoring instance: {instanceId}");
    }

    public void StopMonitoring(string instanceId)
    {
        if (_serviceThreads.TryRemove(instanceId, out var cts))
        {
            cts.Cancel();
            _logger.LogInformation($"Stopped monitoring instance: {instanceId}");
        }
        else
        {
            _logger.LogError($"Instance {instanceId} is not currently being monitored");
        }
    }

    private async Task MonitorInstanceAsync(string instanceId, string onlineTrigger, CancellationToken cancellationToken)
    {
        string kgsmPath = _settingsManager.Settings.Kgsm.Path ?? string.Empty;

        if (kgsmPath == string.Empty)
            throw new ArgumentNullException(nameof(kgsmPath));

        var processStartInfo = new ProcessStartInfo()
        {
            FileName = kgsmPath,
            Arguments = $"--instance {instanceId} --logs",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = processStartInfo };
        process.OutputDataReceived += async (sender, e) =>
            await HandleOutputAsync(instanceId, e.Data, onlineTrigger);
        
        process.ErrorDataReceived += (sender, e) =>
            _logger.LogError($"Instance {instanceId} error: {e.Data}");

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        while (!cancellationToken.IsCancellationRequested && !process.HasExited)
        {
            await Task.Delay(500, cancellationToken);
        }

        if (!process.HasExited)
        {
            process.Kill();
        }

        _logger.LogInformation($"Monitoring thread for Instance: {instanceId} has exited");
    }

    private async Task HandleOutputAsync(string instanceId, string? output, string onlineTrigger)
    {
        if (output == null || !output.Contains(onlineTrigger))
            return;

        _logger.LogInformation($"Instance {instanceId} started");
        
        await _discordNotifier.OnRunningStatusUpdated(instanceId, RunningStatus.Online);

        // Once the instance has been marked as "started" the watchdog doesn't
        // need to do anything else, so it can shutdown.
        StopMonitoring(instanceId);
    }
}
