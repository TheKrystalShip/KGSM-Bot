using Discord;
using Discord.WebSocket;

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
    public async Task OnRunningStatusUpdated(string instanceId, RunningStatus runningStatus)
    {
        string discordChannelId = _settingsManager.GetInstance(instanceId)?.ChannelId ?? string.Empty;

        if (discordChannelId == string.Empty)
        {
            _logger.LogError($"Failed to get discordChannelId for instance: {instanceId}");
            return;
        }

        string emote = _settingsManager.GetStatus(runningStatus) ?? string.Empty;

        if (emote == string.Empty)
        {
            _logger.LogError($"Failed to get new status emote from settings file. New status: {runningStatus}");
            return;
        }

        if (_discordClient.GetChannel(ulong.Parse(discordChannelId)) is not SocketGuildChannel discordChannel)
        {
            _logger.LogError($"Failed to get SocketGuildChannel with ID: {discordChannelId}");
            return;
        }

        if (discordChannel is IMessageChannel messageChannel)
        {
            var sentMessage = await messageChannel.SendMessageAsync($"New status: {runningStatus}");

            // Fire and forget message deletion
            _ = Task.Run(async () => {
                try
                {
                    await Task.Delay(10000);
                    await sentMessage.DeleteAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete the temporary message");
                }
            });
        }

        string newChannelStatusName = $"{emote}{instanceId}";
        _logger.LogInformation($"New status for {instanceId}: {runningStatus}");

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