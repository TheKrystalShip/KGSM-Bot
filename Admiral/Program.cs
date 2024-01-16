using Discord;
using Discord.WebSocket;
using TheKrystalShip.Admiral.Domain;
using TheKrystalShip.Admiral.Tools;
using TheKrystalShip.Logging;

namespace TheKrystalShip.Admiral
{
    public class Program
    {
        const string REGISTER_COMMANDS_ARG = "--register-commands";

        private readonly Logger<Program> _logger;
        private readonly SlashCommandHandler _commandHandler;
        private readonly DiscordSocketClient _client;

        public static void Main(string[] args)
            => new Program().RunAsync(args).GetAwaiter().GetResult();

        public Program()
        {
            _logger = new();
            _commandHandler = new();
            _client = new(new DiscordSocketConfig()
            {
                // https://discord.com/developers/docs/topics/gateway#gateway-intents
                GatewayIntents =
                        GatewayIntents.Guilds |
                        GatewayIntents.GuildMessages
            });

        }

        public async Task RunAsync(string[] args)
        {
            _commandHandler.RunningStatusUpdated += OnRunningStatusUpdated;

            _client.SlashCommandExecuted += _commandHandler.OnSlashCommandExecuted;
            _client.Log += OnClientLog;
            _client.Ready += OnClientReadyAsync;

            // Register slash commands if program was launched with arg.
            if (args.Length > 0 && args[0] == REGISTER_COMMANDS_ARG)
            {
                _client.Ready += OnRegisterSlashCommands;
            }

            await _client.LoginAsync(TokenType.Bot, AppSettings.Get("discord:token"));
            await _client.StartAsync();

            // Stop program from exiting
            await Task.Delay(Timeout.Infinite);
        }

        private async Task OnRunningStatusUpdated(RunningStatusUpdatedArgs args)
        {
            string game = args.Game;
            RunningStatus newStatus = args.RunningStatus;

            string discordChannelId = AppSettings.Get($"discord:channelIds:{game}");

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

            if (_client.GetChannel(ulong.Parse(discordChannelId)) is not SocketGuildChannel discordChannel)
            {
                _logger.LogError($"Failed to get SocketGuildChannel with ID: {discordChannelId}");
                return;
            }

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

        private Task OnClientLog(LogMessage logMessage)
        {
            _logger.LogInformation(logMessage.ToString());
            return Task.CompletedTask;
        }

        private async Task OnClientReadyAsync()
        {
            await _client.SetGameAsync("over servers 👀", null, ActivityType.Watching);
        }

        // Doesn't need to run every time the bot starts up, just once is enough
        // If commands needs changing, they all need to be de-registered firsts, then
        // registered again.
        private async Task OnRegisterSlashCommands()
        {
            // Commands are built for a specific guild, global commands require a lot higher
            // level of permissions and they are not needed for our use case.
            string guildId = AppSettings.Get("discord:guildId");

            if (guildId == string.Empty)
            {
                _logger.LogError("Guild ID is empty, exiting...");
                return;
            }

            SocketGuild guild = _client.GetGuild(ulong.Parse(guildId));

            if (guild is null)
            {
                _logger.LogError("Failed to get guild from client, exiting");
                return;
            }

            List<ApplicationCommandProperties> commandsToRegister = SlashCommandsBuilder.GetCommands();

            try
            {
                // This takes time and will block the gateway
                await guild.BulkOverwriteApplicationCommandAsync([.. commandsToRegister]);
            }
            catch (Exception e)
            {
                _logger.LogError(e);
            }
        }
    }
}
