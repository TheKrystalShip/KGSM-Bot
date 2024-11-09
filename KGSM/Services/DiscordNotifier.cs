using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using TheKrystalShip.KGSM.Domain;
using TheKrystalShip.Logging;

namespace TheKrystalShip.KGSM.Services;

public class DiscordNotifier
{
    private readonly DiscordChannelRegistry _discordChannelRegistry;
    private readonly DiscordSocketClient _discordClient;
    private readonly Logger<DiscordNotifier> _logger;
    private readonly IConfiguration _configuration;

    public DiscordNotifier(
        DiscordChannelRegistry discordChannelRegistry,
        DiscordSocketClient discordSocketClient,
        IConfiguration configuration
    )
    {
        _discordChannelRegistry = discordChannelRegistry;
        _discordClient = discordSocketClient;
        _configuration = configuration;
        _logger = new();
    }

    /// <summary>
    /// Updates the matching discord channel for a given service/game to
    /// reflect the new status provided
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public async Task OnRunningStatusUpdated(RunningStatusUpdatedArgs args)
    {
        string game = args.Game.internalName;
        RunningStatus newStatus = args.RunningStatus;

        string discordChannelId = _configuration[$"games:{game}:channelId"] ?? string.Empty;

        if (discordChannelId == string.Empty)
        {
            _logger.LogError($"Failed to get discordChannelId for game: {game}");
            return;
        }

        string emote = _configuration[$"discord:status:{newStatus}"] ?? string.Empty;

        if (emote == string.Empty)
        {
            _logger.LogError($"Failed to get new status emote from settings file. New status: {newStatus}");
            return;
        }

        if (_discordClient.GetChannel(ulong.Parse(discordChannelId)) is not SocketGuildChannel discordChannel)
        {
            _logger.LogError($"Failed to get SocketGuildChannel with ID: {discordChannelId}");
            return;
        }

        if (discordChannel is IMessageChannel messageChannel)
            await messageChannel.SendMessageAsync($"New status: {newStatus}");

        string newChannelStatusName = $"{emote}{game}";
        _logger.LogInformation($"New status for {game}: {newStatus}");

        try
        {
            // Could fail from discord rate limit, nothing we can do about it atm, so just log it and move on
            await discordChannel.ModifyAsync((channel) => channel.Name = newChannelStatusName);
        }
        catch (Exception e)
        {
            _logger.LogError(e);
        }
    }

    public async Task OnInstanceInstalledAsync(ulong guildId, string instanceId)
    {
        string instancesCategoryId = _configuration[$"discord:instancesCategoryId"] ?? string.Empty;

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

        _logger.LogInformation($"Created new TextChannel with ID: {newTextChannel.Id}");

        await _discordChannelRegistry.AddOrUpdateChannelAsync(
                instanceId: instanceId,
                displayName: System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(instanceId),
                channelId: newTextChannel.Id.ToString()
            );
    }

    public async Task OnInstanceUninstalledAsync(ulong guildId, string instanceId)
    {
        if (_discordClient.GetGuild(guildId) is not SocketGuild socketGuild)
        {
            _logger.LogError($"Failed to load guild with ID: {guildId}");
            return;
        }

        string discordChannelId = _configuration[$"games:{instanceId}:channelId"] ?? string.Empty;

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
        await _discordChannelRegistry.RemoveChannelAsync(instanceId);

        _logger.LogInformation($"Deleted TextChannel '{instanceId}' with ID: {textChannel.Id}");
    }
}
