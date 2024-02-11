using Discord;
using Discord.WebSocket;
using TheKrystalShip.Admiral.Domain;
using TheKrystalShip.Admiral.Tools;
using TheKrystalShip.Logging;

namespace TheKrystalShip.Admiral.Services
{
    public class DiscordNotifier
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly ILogger<DiscordNotifier> _logger;

        public DiscordNotifier(DiscordSocketClient discordSocketClient)
        {
            _discordClient = discordSocketClient;
            _logger = new Logger<DiscordNotifier>();
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

            string discordChannelId = AppSettings.Get($"games:{game}:channelId");

            if (discordChannelId == string.Empty)
            {
                _logger.LogError($"Failed to get discordChannelId for game: {game}");
                return;
            }

            string emote = AppSettings.Get($"discord:status:{newStatus}");

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
}
