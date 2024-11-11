using Discord;
using Discord.WebSocket;

using TheKrystalShip.KGSM.Domain;
using TheKrystalShip.Logging;

namespace TheKrystalShip.KGSM.Services;

public class DiscordNotifier
{
    private readonly DiscordSocketClient _discordClient;
    private readonly Logger<DiscordNotifier> _logger;
    private readonly AppSettingsManager _settingsManager;

    public DiscordNotifier(DiscordSocketClient discordSocketClient, AppSettingsManager settingsManager)
    {
        _discordClient = discordSocketClient;
        _settingsManager = settingsManager;
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
        string instanceId = args.InstanceId;
        RunningStatus newStatus = args.RunningStatus;

        string discordChannelId = _settingsManager.GetInstance(instanceId)?.ChannelId ?? string.Empty;

        if (discordChannelId == string.Empty)
        {
            _logger.LogError($"Failed to get discordChannelId for instance: {instanceId}");
            return;
        }

        string emote = _settingsManager.GetStatus(newStatus) ?? string.Empty;

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

        string newChannelStatusName = $"{emote}{instanceId}";
        _logger.LogInformation($"New status for {instanceId}: {newStatus}");

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
