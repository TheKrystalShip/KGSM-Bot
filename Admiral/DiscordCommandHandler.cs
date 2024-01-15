using Discord;
using Discord.WebSocket;
using TheKrystalShip.Admiral.Domain;
using TheKrystalShip.Admiral.Services;
using TheKrystalShip.Admiral.Tools;
using TheKrystalShip.Logging;

namespace TheKrystalShip.Admiral
{
    public class SlashCommandHandler
    {
        private readonly Logger<SlashCommandHandler> _logger;
        private readonly DiscordSocketClient _client;
        private readonly CommandExecutioner _commandExecutioner;

        public SlashCommandHandler(DiscordSocketClient client, CommandExecutioner commandExecutioner)
        {
            _logger = new();
            _client = client;
            _commandExecutioner = commandExecutioner;
        }

        public async Task HandleCommand(SocketSlashCommand command)
        {
            // All commands require the user to specify a game.
            string game = (string) (command.Data.Options?.First()?.Value ?? "");
            string commandFormatted = (command.Data.Name ?? "") + " " + game;

            _logger.LogInformation($"Executing /{commandFormatted}");

            switch (command.Data.Name)
            {
                case "start":   await HandleGameStartCommandAsync(command, game);
                    break;
                case "stop":    await HandleGameStopCommandAsync(command, game);
                    break;
                case "restart": await HandleGameRestartCommandAsync(command, game);
                    break;
                case "status":  await HandleGameStatusCommandAsync(command, game);
                    break;
                default:        await command.RespondAsync("Could not find command");
                    break;
            }
        }

        private async Task HandleGameStartCommandAsync(SocketSlashCommand command, string game)
        {
            // Respond quickly then handle the task in the background.
            Task respondTask = command.RespondAsync($"Starting {game}...");

            CommandExecutionResult result = _commandExecutioner.Start(game);

            if (result.Status == ExecutionsStatus.Error)
            {
                await command.RespondAsync($"There might have been an error: {result.Output}");
                return;
            }

            await respondTask;
            await UpdateDiscordChannelAsync(game, GameServerStatus.Online);
        }

        private async Task HandleGameStopCommandAsync(SocketSlashCommand command, string game)
        {
            // Respond quickly then handle the task in the background.
            Task respondTask = command.RespondAsync($"Stopping {game}...");
            CommandExecutionResult result = _commandExecutioner.Stop(game);

            if (result.Status == ExecutionsStatus.Error)
            {
                await command.RespondAsync($"There might have been an error: {result.Output}");
                return;
            }

            await respondTask;
            await UpdateDiscordChannelAsync(game, GameServerStatus.Offline);
        }

        private async Task HandleGameRestartCommandAsync(SocketSlashCommand command, string game)
        {
            // Respond quickly then handle the task in the background.
            Task respondTask = command.RespondAsync($"Restarting {game}...");
            CommandExecutionResult result = _commandExecutioner.Restart(game);

            if (result.Status == ExecutionsStatus.Error)
            {
                await command.RespondAsync($"There might have been an error: {result.Output}");
                return;
            }

            await respondTask;
        }

        private async Task HandleGameStatusCommandAsync(SocketSlashCommand command, string game)
        {
            CommandExecutionResult result = _commandExecutioner.Status(game);

            if (result.Status == ExecutionsStatus.Error)
            {
                await command.RespondAsync($"There might have been an error: {result.Output}");
                return;
            }

            if (result.Output != string.Empty)
            {
                await command.RespondAsync(result.Output);
                return;
            }
        }

        /// <summary>
        /// Updates the game's discord channel to reflect the new service status
        /// </summary>
        /// <param name="game">Game channel name</param>
        /// <param name="newStatus">New service status</param>
        /// <returns></returns>
        private Task UpdateDiscordChannelAsync(string game, GameServerStatus newStatus)
        {
            string discordChannelId = AppSettings.Get($"discord:channelIds:{game}");

            if (discordChannelId == string.Empty)
            {
                _logger.LogError($"Failed to fetch discord channelId from settings file for game: {game}");
                return Task.CompletedTask;
            }

            string emote = AppSettings.Get($"discord:status:{newStatus}");

            if (emote == string.Empty)
            {
                _logger.LogError($"Failed to fetch new status emote from settings file. New status: {newStatus}");
                return Task.CompletedTask;
            }

            if (_client.GetChannel(ulong.Parse(discordChannelId)) is not SocketGuildChannel discordChannel)
            {
                _logger.LogError($"Failed to get discord channel with ID: {discordChannelId}");
                return Task.CompletedTask;
            }

            string newChannelStatusName = $"{emote}{game}";
            _logger.LogInformation($"New status for {game}: {newStatus}");

            // Change channel name to reflect new GameServerStatus
            // Fire and forget, otherwise it blocks the gateway
            _ = discordChannel.ModifyAsync((channel) => channel.Name = newChannelStatusName);

            return Task.CompletedTask;
        }

        // Doesn't need to run every time the bot starts up, just once is enough
        // If commands needs changing, they all need to be de-registered firsts, then
        // registered again.
        public async Task RegisterSlashCommands()
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

            List<ApplicationCommandProperties> commandsToRegister = [];

            SlashCommandBuilder gameStartCommand = new SlashCommandBuilder()
                .WithName("start")
                .WithDescription("Start a game server")
                .AddOption("game", ApplicationCommandOptionType.String, "Game server name", isRequired: true);

            SlashCommandBuilder gameStopCommand = new SlashCommandBuilder()
                .WithName("stop")
                .WithDescription("Stop a game server")
                .AddOption("game", ApplicationCommandOptionType.String, "Game server name", isRequired: true);

            SlashCommandBuilder gameRestartCommand = new SlashCommandBuilder()
                .WithName("restart")
                .WithDescription("Restart a game server")
                .AddOption("game", ApplicationCommandOptionType.String, "Game server name", isRequired: true);

            SlashCommandBuilder gameStatusCommand = new SlashCommandBuilder()
                .WithName("status")
                .WithDescription("Check the status of a game server")
                .AddOption("game", ApplicationCommandOptionType.String, "Game server name", isRequired: true);

            commandsToRegister.Add(gameStartCommand.Build());
            commandsToRegister.Add(gameStopCommand.Build());
            commandsToRegister.Add(gameRestartCommand.Build());
            commandsToRegister.Add(gameStatusCommand.Build());

            try
            {
                // This takes time and will block the gateway
                foreach (var command in commandsToRegister)
                {
                    await guild.CreateApplicationCommandAsync(command);
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception);
            }
        }
    }
}
