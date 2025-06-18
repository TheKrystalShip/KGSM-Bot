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

    public void StartMonitoring(string instanceName)
    {
        if (_serviceThreads.ContainsKey(instanceName))
        {
            _logger.LogError($"Instance {instanceName} is already being monitored");
            return;
        }

        var trigger = _settingsManager.GetTrigger(instanceName);

        if (trigger == null)
        {
            _logger.LogError($"Triggers for instance: {instanceName} is null");
            return;
        }

        var cts = new CancellationTokenSource();
        _serviceThreads[instanceName] = cts;

        Task.Run(() => MonitorInstanceAsync(instanceName, trigger, cts.Token));
        _logger.LogInformation($"Started monitoring instance: {instanceName}");
    }

    public void StopMonitoring(string instanceName)
    {
        if (_serviceThreads.TryRemove(instanceName, out var cts))
        {
            cts.Cancel();
            _logger.LogInformation($"Stopped monitoring instance: {instanceName}");
        }
        else
        {
            _logger.LogError($"Instance {instanceName} is not currently being monitored");
        }
    }

    private async Task MonitorInstanceAsync(
        string instanceName,
        string onlineTrigger,
        CancellationToken cancellationToken
    )
    {
        string kgsmPath = _settingsManager.Settings.Kgsm.Path ?? string.Empty;

        if (kgsmPath == string.Empty)
            throw new ArgumentNullException(nameof(kgsmPath));

        var processStartInfo = new ProcessStartInfo()
        {
            FileName = kgsmPath,
            Arguments = $"--instance {instanceName} --logs --follow",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = new Process { StartInfo = processStartInfo };
        process.OutputDataReceived += async (sender, e) =>
            await HandleOutputAsync(instanceName, e.Data, onlineTrigger);

        process.ErrorDataReceived += (sender, e) =>
            _logger.LogError($"Instance {instanceName} error: {e.Data}");

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            // Wait for the process to exit asynchronously.
            // This will observe cancellation as well.
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation($"Cancellation requested for instance {instanceName}.");
            // If cancellation occurs before process exit, kill the entire process tree.
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                _logger.LogInformation($"Killed entire process tree for instance {instanceName}.");
            }
        }
    }

    private async Task HandleOutputAsync(string instanceName, string? output, string onlineTrigger)
    {
        if (output == null || !output.Contains(onlineTrigger))
            return;

        _logger.LogInformation($"Instance {instanceName} started");

        await _discordNotifier.OnRunningStatusUpdated(instanceName, RunningStatus.Online);

        // Once the instance has been marked as "started" the watchdog doesn't
        // need to do anything else, so it can shutdown.
        StopMonitoring(instanceName);
    }
}
