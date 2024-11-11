using Microsoft.Extensions.Configuration;
using System.Text.Json;

using TheKrystalShip.Logging;

namespace TheKrystalShip.KGSM;

public class AppSettingsManager
{
    private readonly IConfigurationRoot _configuration;
    private readonly string _filePath;
    public AppSettings Settings { get; private set; }

    public AppSettingsManager(string configFilePath)
    {
        _filePath = configFilePath;

        // Load the configuration from appsettings.json
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(_filePath, optional: false, reloadOnChange: true);
        _configuration = builder.Build();

        // Bind the configuration to the AppSettings model
        Settings = new AppSettings();
        _configuration.Bind(Settings);
    }

    /// <summary>
    /// Adds a new instance or update an existing one and persists the change.
    /// </summary>
    public void AddOrUpdateInstance(string key, InstanceSettings instance)
    {
        Settings.Instances[key] = instance;
        SaveSettings();
    }

    /// <summary>
    /// Removes an instance from the "instances" section and persists the change.
    /// </summary>
    public void RemoveInstance(string key)
    {
        if (Settings.Instances.ContainsKey(key))
        {
            Settings.Instances.Remove(key);
            SaveSettings();
        }
    }

    public string GetStatus(RunningStatus rs)
    {
        return rs switch
        {
            RunningStatus.Online => Settings.Discord.Status.Online,
            RunningStatus.Offline => Settings.Discord.Status.Offline,
            RunningStatus.Error => Settings.Discord.Status.Error,
            RunningStatus.NeedsUpdate => Settings.Discord.Status.NeedsUpdate,
            RunningStatus.Uninstalled => Settings.Discord.Status.Uninstalled,
            _ => string.Empty
        };
    }

    public BlueprintSettings GetTrigger(string instanceId)
    {
        return Settings.Blueprints[Settings.Instances[instanceId].Blueprint];
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

