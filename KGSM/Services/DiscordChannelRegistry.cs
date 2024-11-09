using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.IO;

using TheKrystalShip.Logging;

namespace TheKrystalShip.KGSM.Services;

public class DiscordChannelRegistry
{
    private readonly IConfiguration _configuration;
    private readonly string _configFilePath;
    private Dictionary<string, GameChannel> _channelMap;
    private readonly Logger<DiscordChannelRegistry> _logger;

    public DiscordChannelRegistry(IConfiguration configuration)
    {
        _configuration = configuration;
        _configFilePath = "appsettings.json";
        _logger = new();

        _channelMap = _configuration.GetSection("games").Get<Dictionary<string, GameChannel>>() ?? new();
    }

    // Represents the channel configuration for each game
    public class GameChannel
    {
        public string DisplayName { get; set; } = "";
        public string ChannelId { get; set; } = "";
    }

    public Dictionary<string, GameChannel> GetChannels()
    {
        return _channelMap;
    }

    public async Task AddOrUpdateChannelAsync(string instanceId, string displayName, string channelId)
    {
        if (_channelMap.ContainsKey(instanceId))
        {
            _channelMap[instanceId].DisplayName = displayName;
            _channelMap[instanceId].ChannelId = channelId;

            _logger.LogInformation($"Updated - Channel: {instanceId}, DisplayName: {displayName}, ChannelId: {channelId}");
        }
        else
        {
            _channelMap[instanceId] = new GameChannel { DisplayName = displayName, ChannelId = channelId };

            _logger.LogInformation($"Added - Channel: {instanceId}, DisplayName: {displayName}, ChannelId: {channelId}");
        }

        await SaveConfigurationAsync();
    }

    public async Task RemoveChannelAsync(string instanceId)
    {
        if (_channelMap.Remove(instanceId))
        {
            _logger.LogInformation($"Removed - Channel: {instanceId}");
            await SaveConfigurationAsync();
        }
        else
        {
            _logger.LogError($"Channel {instanceId} not found");
        }
    }

    private async Task SaveConfigurationAsync()
    {
        string jsonString = await File.ReadAllTextAsync(_configFilePath);
        var jsonDocument = JsonDocument.Parse(jsonString);

        // Parse the JSON into a dictionary
        var configDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonDocument.RootElement.GetRawText()) ?? new();

        configDictionary["games"] = _channelMap;

        // Serialize the updated config back to JSON
        var updatedJson = JsonSerializer.Serialize(configDictionary, new JsonSerializerOptions { WriteIndented = true });

        // Write the updated JSON to the config file
        await File.WriteAllTextAsync(_configFilePath, updatedJson);
    }
}
