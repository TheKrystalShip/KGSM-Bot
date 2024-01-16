using Discord;

namespace TheKrystalShip.Admiral.Tools
{
    // https://discordnet.dev/guides/int_basics/application-commands/intro.html
    public static class SlashCommandsBuilder
    {
        private static readonly List<ApplicationCommandProperties> _commandList = [];

        public static List<ApplicationCommandProperties> GetCommands()
        {
            _commandList.Add(new SlashCommandBuilder()
                .WithName("start")
                .WithDescription("Start a game server")
                .AddOption("game", ApplicationCommandOptionType.String, "Game server name", isRequired: true)
                .Build()
            );

            _commandList.Add(new SlashCommandBuilder()
                .WithName("stop")
                .WithDescription("Stop a game server")
                .AddOption("game", ApplicationCommandOptionType.String, "Game server name", isRequired: true)
                .Build()
            );

            _commandList.Add(new SlashCommandBuilder()
                .WithName("restart")
                .WithDescription("Restart a game server")
                .AddOption("game", ApplicationCommandOptionType.String, "Game server name", isRequired: true)
                .Build()
            );

            _commandList.Add(new SlashCommandBuilder()
                .WithName("status")
                .WithDescription("Get a more detailed status of a game server")
                .AddOption("game", ApplicationCommandOptionType.String, "Game server name", isRequired: true)
                .Build()
            );

            _commandList.Add(new SlashCommandBuilder()
                .WithName("autostart")
                .WithDescription("Set if a game should automatically start on system reboot")
                .AddOption("game", ApplicationCommandOptionType.String, "Game server name", isRequired: true)
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
                .WithName("is-active")
                .WithDescription("Checks if a game server is currently running")
                .AddOption("game", ApplicationCommandOptionType.String, "Game server name", isRequired: true)
                .Build()
            );

            _commandList.Add(new SlashCommandBuilder()
                .WithName("is-enabled")
                .WithDescription("Checks if a game server is set to autostart on system reboot")
                .AddOption("game", ApplicationCommandOptionType.String, "Game server name", isRequired: true)
                .Build()
            );

            return _commandList;
        }
    }
}
