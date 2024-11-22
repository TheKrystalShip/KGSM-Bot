using Discord.Rest;
using Discord.WebSocket;

using TheKrystalShip.Logging;

namespace TheKrystalShip.KGSM.Services;

public class DiscordChannelRegistry
{
    private readonly DiscordSocketClient _discordClient;
    private readonly Logger<DiscordChannelRegistry> _logger;
    private readonly AppSettingsManager _settingsManager;

    public DiscordChannelRegistry(DiscordSocketClient discordClient, AppSettingsManager settingsManager)
    {
        _discordClient = discordClient;
        _logger = new();
        _settingsManager = settingsManager;
    }

    public async Task AddOrUpdateChannelAsync(ulong guildId, string blueprint, string instanceId)
    {
        ulong instancesCategoryId = _settingsManager.Discord.InstancesCategoryId;

        if (_discordClient.GetGuild(guildId) is not SocketGuild socketGuild)
        {
            _logger.LogError($"Failed to load guild with ID: {guildId}");
            return;
        }

        if (socketGuild.GetCategoryChannel(instancesCategoryId) is not SocketCategoryChannel categoryChannel)
        {
            _logger.LogError($"Failed to load category channel with ID: {instancesCategoryId}");
            return;
        }

        string channelId = string.Empty;

        if (_settingsManager.Instances.ContainsKey(instanceId))
        {
            channelId = _settingsManager.Instances[instanceId].ChannelId;
        }
        else
        {
            RestTextChannel newTextChannel = await socketGuild
                .CreateTextChannelAsync(instanceId, props => props.CategoryId = categoryChannel.Id);

            channelId = newTextChannel.Id.ToString();
        }

        InstanceSettings instanceSettings = new()
        {
            ChannelId = channelId,
            Blueprint = blueprint
        };

        _settingsManager.AddOrUpdateInstance(instanceId, instanceSettings);

        _logger.LogInformation($"Added - {instanceSettings}");
    }

    public async Task RemoveChannelAsync(ulong guildId, string instanceId)
    {
        if (_discordClient.GetGuild(guildId) is not SocketGuild socketGuild)
        {
            _logger.LogError($"Failed to load guild with ID: {guildId}");
            return;
        }

        string discordChannelId = _settingsManager.GetInstance(instanceId)?.ChannelId ?? string.Empty;

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
        
        if (_settingsManager.Discord.RemoveChannelOnInstanceDeletion)
        {
            await textChannel.DeleteAsync();
            _settingsManager.RemoveInstance(instanceId);
            _logger.LogInformation($"Removed - Channel: {instanceId}, ID: {textChannel.Id}");
        }
        else
        {
            _logger.LogInformation($"Preserving Channel: {instanceId}, ID: {textChannel.Id}");
        }
    }
}
