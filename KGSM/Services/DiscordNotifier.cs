using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using TheKrystalShip.KGSM.Domain;
using TheKrystalShip.Logging;

namespace TheKrystalShip.KGSM.Services;

public class DiscordNotifier
{
    private readonly DiscordSocketClient _discordClient;
    private readonly Logger<DiscordNotifier> _logger;
    private readonly IConfiguration _configuration;

    public DiscordNotifier(
        DiscordSocketClient discordSocketClient,
        IConfiguration configuration
    )
    {
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
}
