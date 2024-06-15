using Discord;
using TheKrystalShip.Admiral.Domain;

namespace TheKrystalShip.Admiral.Tools
{
    // https://discordnet.dev/guides/int_basics/application-commands/intro.html
    public static class SlashCommandsBuilder
    {
        private static readonly List<ApplicationCommandProperties> _commandList = [];

        /// <summary>
        /// Get a list of all the commands the bot will use
        /// </summary>
        /// <returns>The list of commands</returns>
        public static List<ApplicationCommandProperties> GetCommandList()
        {
            _commandList.Add(new SlashCommandBuilder()
                .WithName(Command.START)
                .WithDescription("Start a game server")
                .AddOption("game", ApplicationCommandOptionType.Channel, "Game server name", isRequired: true)
                .Build()
            );

            _commandList.Add(new SlashCommandBuilder()
                .WithName(Command.STOP)
                .WithDescription("Stop a game server")
                .AddOption("game", ApplicationCommandOptionType.Channel, "Game server name", isRequired: true)
                .Build()
            );

            _commandList.Add(new SlashCommandBuilder()
                .WithName(Command.RESTART)
                .WithDescription("Restart a game server")
                .AddOption("game", ApplicationCommandOptionType.Channel, "Game server name", isRequired: true)
                .Build()
            );

            _commandList.Add(new SlashCommandBuilder()
                .WithName(Command.STATUS)
                .WithDescription("Get a more detailed status of a game server")
                .AddOption("game", ApplicationCommandOptionType.Channel, "Game server name", isRequired: true)
                .Build()
            );

            _commandList.Add(new SlashCommandBuilder()
                .WithName(Command.SET_AUTOSTART)
                .WithDescription("Set if a game should automatically start on system reboot")
                .AddOption("game", ApplicationCommandOptionType.Channel, "Game server name", isRequired: true)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("value")
                    .WithDescription("True or False")
                    .WithRequired(true)
                    .AddChoice("true", 1)
                    .AddChoice("false", 0)
                    .WithType(ApplicationCommandOptionType.Integer)
                )
                .Build()
            );

            _commandList.Add(new SlashCommandBuilder()
                .WithName(Command.IS_AUTOSTART_ENABLED)
                .WithDescription("Checks if a game server is set to autostart on system reboot")
                .AddOption("game", ApplicationCommandOptionType.Channel, "Game server name", isRequired: true)
                .Build()
            );

            _commandList.Add(new SlashCommandBuilder()
                .WithName(Command.IS_ONLINE)
                .WithDescription("Checks if a game server is currently running")
                .AddOption("game", ApplicationCommandOptionType.Channel, "Game server name", isRequired: true)
                .Build()
            );

            _commandList.Add(new SlashCommandBuilder()
                .WithName(Command.CHECK_FOR_UPDATE)
                .WithDescription("Check if there's an update for a game")
                .AddOption("game", ApplicationCommandOptionType.Channel, "Game server name", isRequired: true)
                .Build()
            );

            _commandList.Add(new SlashCommandBuilder()
                .WithName(Command.GET_LOGS)
                .WithDescription("Get the last 'n' lines of a game's log")
                .AddOption("game", ApplicationCommandOptionType.Channel, "Game server name", isRequired: true)
                .AddOption("lines", ApplicationCommandOptionType.Integer, "Number of lines", isRequired: false)
                .Build()
            );

            _commandList.Add(new SlashCommandBuilder()
                .WithName(Command.GET_IP)
                .WithDescription("Get the server's IP address")
                .Build()
            );

            return _commandList;
        }
    }
}
