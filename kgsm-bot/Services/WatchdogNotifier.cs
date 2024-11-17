﻿using System.Collections.Concurrent;
using System.Diagnostics;

using TheKrystalShip.KGSM.Domain;

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

        var triggers = _settingsManager.GetTrigger(instanceId);

        if (triggers == null)
        {
            _logger.LogError($"Triggers for instance: {instanceId} is null");
            return;
        }
        
        if (triggers.OnlineTrigger == string.Empty)
        {
            _logger.LogError($"OnlineTrigger for {instanceId} is empty, aborting");
            return;
        }
        
        if (triggers.OfflineTrigger == string.Empty)
        {
            _logger.LogError($"OfflineTrigger for {instanceId} is empty, aborting");
            return;
        }

        var cts = new CancellationTokenSource();
        _serviceThreads[instanceId] = cts;

        Task.Run(() => MonitorInstanceAsync(instanceId, triggers.OnlineTrigger, triggers.OfflineTrigger, cts.Token));
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

    private async Task MonitorInstanceAsync(string instanceId, string onlineTrigger, string offlineTrigger, CancellationToken cancellationToken)
    {
        string kgsmPath = _settingsManager.Settings.KgsmPath ?? string.Empty;

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
            await HandleOutputAsync(instanceId, e.Data, onlineTrigger, offlineTrigger);
        
        process.ErrorDataReceived += (sender, e) =>
            _logger.LogError($"Instance {instanceId} error: {e.Data}");

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        // Assume instance is offline initially
        bool previousStatus = false;
        bool currentStatus = previousStatus;

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

    private async Task HandleOutputAsync(string instanceId, string? output, string onlineTrigger, string offlineTrigger)
    {
        if (output == null)
            return;

        // Assume instance is offline initially
        bool previousStatus = false;
        bool currentStatus = previousStatus;

        // Trigger check
        if (output.Contains(onlineTrigger))
        {
            currentStatus = true;
        }
        else if (output.Contains(offlineTrigger))
        {
            currentStatus = false;
        }

        if (currentStatus != previousStatus)
        {
            previousStatus = currentStatus;
            _logger.LogInformation($"Instance {instanceId} status changed: {(currentStatus ? "Online" : "Offline")}");
            
            var rsua = new RunningStatusUpdatedArgs(new(instanceId), currentStatus ? RunningStatus.Online : RunningStatus.Offline);
            await _discordNotifier.OnRunningStatusUpdated(rsua);
        }
    }
}
