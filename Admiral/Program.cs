using Discord;
using Discord.WebSocket;
using TheKrystalShip.Admiral.Services;
using TheKrystalShip.Admiral.Tools;
using TheKrystalShip.Logging;

namespace TheKrystalShip.Admiral
{
    public class Program
    {
        const string REGISTER_COMMANDS_ARG = "--register-commands";

        private readonly Logger<Program> _logger;

        public static void Main(string[] args)
            => new Program().RunAsync(args).GetAwaiter().GetResult();

        public Program()
        {
            _logger = new();
        }

        public async Task RunAsync(string[] args)
        {
            DiscordSocketClient client = new(new DiscordSocketConfig()
                {
                    // https://discord.com/developers/docs/topics/gateway#gateway-intents
                    GatewayIntents =
                        GatewayIntents.Guilds |
                        GatewayIntents.GuildMessages
                });

            SlashCommandHandler commandHandler = new(client, new CommandExecutioner());

            client.SlashCommandExecuted += commandHandler.HandleCommand;
            client.Log += OnClientLog;
            client.Ready += () => OnClientReadyAsync(client);

            // Register slash commands if program was launched with arg.
            if (args.Length > 0 && args[0] == REGISTER_COMMANDS_ARG)
            {
                client.Ready += commandHandler.RegisterSlashCommands;
            }

            await client.LoginAsync(TokenType.Bot, AppSettings.Get("discord:token"));
            await client.StartAsync();

            // Stop program from exiting
            await Task.Delay(Timeout.Infinite);
        }

        private Task OnClientLog(LogMessage logMessage)
        {
            _logger.LogInformation(logMessage.ToString());
            return Task.CompletedTask;
        }

        private async Task OnClientReadyAsync(DiscordSocketClient client)
        {
            await client.SetGameAsync("over servers 👀", null, ActivityType.Watching);

            // Leave commented unless you know what you're doing
            // await _services.GetRequiredService<SlashCommandHandler>().RegisterSlashCommands();
        }
    }
}
