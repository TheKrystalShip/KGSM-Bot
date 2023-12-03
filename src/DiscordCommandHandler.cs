using Discord;
using Discord.WebSocket;
using TheKrystalShip.Admiral.Domain;
using TheKrystalShip.Admiral.Services;
using TheKrystalShip.Admiral.Tools;

namespace TheKrystalShip.Admiral
{
    public class DiscordCommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandExecutioner _commandExecutioner;

        public DiscordCommandHandler(DiscordSocketClient client, CommandExecutioner commandExecutioner)
        {
            _client = client;
            _commandExecutioner = commandExecutioner;
        }

        public async Task HandleCommand(SocketSlashCommand command)
        {
            // All commands require the user to specify a game.
            string game = (string)command.Data.Options.First().Value;

            switch (command.Data.Name)
            {
                case "start":
                    await HandleGameStartCommandAsync(command, game);
                    break;
                case "stop":
                    await HandleGameStopCommandAsync(command, game);
                    break;
                case "restart":
                    await HandleGameRestartCommandAsync(command, game);
                    break;
                case "status":
                    await HandleGameStatusCommandAsync(command, game);
                    break;
            }
        }

        private async Task HandleGameStartCommandAsync(SocketSlashCommand command, string game)
        {
            CommandExecutionResult result = _commandExecutioner.Execute(Commands.Start(game));

            await command.RespondAsync(result.Output);
        }

        private async Task HandleGameStopCommandAsync(SocketSlashCommand command, string game)
        {
            CommandExecutionResult result = _commandExecutioner.Execute(Commands.Stop(game));

            await command.RespondAsync(result.Output);
        }

        private async Task HandleGameRestartCommandAsync(SocketSlashCommand command, string game)
        {
            CommandExecutionResult result = _commandExecutioner.Execute(Commands.Stop(game));

            await command.RespondAsync(result.Output);
        }

        private async Task HandleGameStatusCommandAsync(SocketSlashCommand command, string game)
        {
            CommandExecutionResult result = _commandExecutioner.Execute(Commands.Status(game));

            await command.RespondAsync(result.Output);
        }

        // Doesn't need to run every time the bot starts up, just once is enough
        // If commands needs changing, they all need to be de-registered firsts, then
        // registered again.
        public async Task RegisterSlashCommands()
        {
            // Commands are built for a specific guild, global commands require a lot higher
            // level of permissions and they are not needed for our use case.
            string guildId = AppSettings.Get("discord:guildId") ?? "";
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
                Console.WriteLine(exception.Message);
            }
        }
    }
}
