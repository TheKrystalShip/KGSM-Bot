using Discord.Rest;
using Discord.WebSocket;

using TheKrystalShip.Logging;

namespace TheKrystalShip.KGSM.Services;

public class DiscordChannelRegistry
{
    private readonly DiscordSocketClient _discordClient;
    private readonly WatchdogNotifier _watchdogNotifier;
    private readonly Logger<DiscordChannelRegistry> _logger;
    private readonly AppSettingsManager _settingsManager;

    public DiscordChannelRegistry(DiscordSocketClient discordClient, WatchdogNotifier watchdogNotifier, AppSettingsManager settingsManager)
    {
        _discordClient = discordClient;
        _watchdogNotifier = watchdogNotifier;
        _logger = new();
        _settingsManager = settingsManager;
    }

    public async Task AddOrUpdateChannelAsync(ulong guildId, string blueprint, string instanceId)
    {
        string instancesCategoryId = _settingsManager.Settings.Discord.InstancesCategoryId ?? string.Empty;

        if (instancesCategoryId == string.Empty)
        {
            _logger.LogError($"Failed to get instancesCategoryId from config file");
            return;
        }

        if (_discordClient.GetGuild(guildId) is not SocketGuild socketGuild)
        {
            _logger.LogError($"Failed to load guild with ID: {guildId}");
            return;
        }

        if (
                socketGuild.GetCategoryChannel(ulong.Parse(instancesCategoryId))
                is not SocketCategoryChannel categoryChannel
            )
        {
            _logger.LogError($"Failed to load category channel with ID: {instancesCategoryId}");
            return;
        }

        RestTextChannel newTextChannel = await socketGuild
            .CreateTextChannelAsync(instanceId, props => props.CategoryId = categoryChannel.Id);

        string channelId = newTextChannel.Id.ToString();

        InstanceSettings instanceSettings = new()
        {
            ChannelId = newTextChannel.Id.ToString(),
            Blueprint = blueprint
        };

        _settingsManager.AddOrUpdateInstance(instanceId, instanceSettings);

        _logger.LogInformation($"Added - {instanceSettings}");

        _watchdogNotifier.StartMonitoring(instanceId);
    }

    public async Task RemoveChannelAsync(ulong guildId, string instanceId)
    {
        if (_discordClient.GetGuild(guildId) is not SocketGuild socketGuild)
        {
            _logger.LogError($"Failed to load guild with ID: {guildId}");
            return;
        }

        string discordChannelId = _settingsManager.Settings.Instances[instanceId]?.ChannelId ?? string.Empty;

        if (discordChannelId == string.Empty)
        {
            _logger.LogError($"Failed to get discordChannelId for game: {instanceId}");
            return;
        }

        var textChannel = socketGuild.TextChannels.FirstOrDefault(
                channel => channel.Id == ulong.Parse(discordChannelId));

        if (textChannel is null)
        {
            _logger.LogError($"Failed to load text channel with name: {instanceId}");
            return;
        }

        await textChannel.DeleteAsync();
        
        _settingsManager.RemoveInstance(instanceId);
        
        _logger.LogInformation($"Removed - Channel: {instanceId}, ID: {textChannel.Id}");

        _watchdogNotifier.StopMonitoring(instanceId);
    }
}
