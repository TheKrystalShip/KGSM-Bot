using Microsoft.Extensions.Configuration;

using System.Text.Json;

using TheKrystalShip.Logging;

namespace TheKrystalShip.KGSM;

public class AppSettingsManager
{
    private readonly IConfigurationRoot _configuration;
    private readonly string _filePath;
    private readonly Logger<AppSettingsManager> _logger;
    public AppSettings Settings { get; private set; }
    public DiscordSettings Discord { get => Settings.Discord; }
    public KgsmSettings Kgsm { get => Settings.Kgsm; }
    public Dictionary<string, InstanceSettings> Instances { get => Settings.Kgsm.Instances; }
    public Dictionary<string, BlueprintSettings> Blueprints { get => Settings.Kgsm.Blueprints; }

    public AppSettingsManager(string configFilePath)
    {
        _filePath = configFilePath;
        _logger = new();

        // Load the configuration from appsettings.json
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(_filePath, optional: false, reloadOnChange: true);
        _configuration = builder.Build();

        // Bind the configuration to the AppSettings model
        Settings = new AppSettings();
        _configuration.Bind(Settings, options =>
        {
            options.ErrorOnUnknownConfiguration = true;
        });
    }

    /// <summary>
    /// Adds a new instance or update an existing one and persists the change.
    /// </summary>
    public void AddOrUpdateInstance(string key, InstanceSettings instance)
    {
        Settings.Kgsm.Instances[key] = instance;
        SaveSettings();
    }

    /// <summary>
    /// Removes an instance from the "instances" section and persists the change.
    /// </summary>
    public void RemoveInstance(string key)
    {
        if (!Settings.Kgsm.Instances.ContainsKey(key))
        {
            _logger.LogError($"Instance {key} not in memory, nothing to remove");
            return;
        }

        Settings.Kgsm.Instances.Remove(key);
        SaveSettings();
    }

    public string GetStatus(RunningStatus rs)
    {
        return rs switch
        {
            RunningStatus.Online => Settings.Discord.Status.Online,
            RunningStatus.Offline => Settings.Discord.Status.Offline,
            RunningStatus.Uninstalled => Settings.Discord.Status.Uninstalled,
            _ => string.Empty
        };
    }

    public string? GetTrigger(string instanceName)
    {
        if (!Settings.Kgsm.Instances.TryGetValue(instanceName, out InstanceSettings? value))
        {
            _logger.LogError($"Instance {instanceName} not in memory");
            return null;
        }

        string instanceBlueprint = value.Blueprint;

        if (instanceBlueprint == string.Empty)
        {
            _logger.LogError($"Instance {instanceName} doesn't have a blueprint defined");
            return null;
        }

        if (!Settings.Kgsm.Blueprints.TryGetValue(instanceBlueprint, out BlueprintSettings? blueprintValue))
        {
            _logger.LogError($"Blueprint {instanceBlueprint} not in memory");
            return null;
        }

        return blueprintValue.OnlineTrigger;
    }

    public InstanceSettings? GetInstance(string instanceName)
    {
        if (!Settings.Kgsm.Instances.TryGetValue(instanceName, out InstanceSettings? value))
        {
            _logger.LogError($"Instance {instanceName} not in memory");
            return null;
        }

        return value;
    }

    /// <summary>
    /// Persists the current settings to the appsettings.json file.
    /// </summary>
    private void SaveSettings()
    {
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(Settings, jsonOptions);
        File.WriteAllText(_filePath, json);
    }
}
