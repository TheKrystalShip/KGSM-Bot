using System.Text;
using Discord;
using Discord.WebSocket;
using TheKrystalShip.Admiral.Services;
using TheKrystalShip.Admiral.Tools;
using TheKrystalShip.Logging;

namespace TheKrystalShip.Admiral
{
    public class Program
    {
        private static bool _useRegisterCommands = false;
        private static bool _useRabbitMq = false;

        private readonly Logger<Program> _logger;
        private readonly SlashCommandHandler _commandHandler;
        private readonly DiscordSocketClient _discordClient;
        private readonly DiscordNotifier _discordNotifier;
        private readonly RabbitMQClient? _rabbitMqClient;

        public static void Main(string[] args)
        {
            foreach (string item in args)
            {
                switch (item)
                {
                    case "-h":
                    case "--help":
                        PrintHelp();
                        return;
                    case "--register-commands":
                        _useRegisterCommands = true;
                        break;
                    case "--rabbitmq":
                        _useRabbitMq = true;
                        break;
                    default:
                        break;
                }
            }

            new Program().RunAsync().GetAwaiter().GetResult();
        }

        public Program()
        {
            _logger = new();
            _commandHandler = new();
            _discordClient = new(new DiscordSocketConfig()
            {
                // https://discord.com/developers/docs/topics/gateway#gateway-intents
                GatewayIntents =
                        GatewayIntents.Guilds |
                        GatewayIntents.GuildMessages
            });

            _discordNotifier = new(_discordClient);

            if (_useRabbitMq)
            {
                _logger.LogInformation("Using RabbitMQ");
                _rabbitMqClient = new(_discordNotifier);
            }
        }

        public async Task RunAsync()
        {
            _discordClient.SlashCommandExecuted += _commandHandler.OnSlashCommandExecuted;
            _discordClient.Log += OnClientLog;
            _discordClient.Ready += OnClientReadyAsync;

            if (_useRegisterCommands)
            {
                _logger.LogInformation("Registering commands to Discord");
                _discordClient.Ready += OnRegisterSlashCommands;
            }

            await _discordClient.LoginAsync(TokenType.Bot, AppSettings.Get("discord:token"));
            await _discordClient.StartAsync();

            if (_useRabbitMq && _rabbitMqClient != null)
            {
                if (await _rabbitMqClient.LoginAsync(AppSettings.Get("rabbitmq:uri")))
                {
                    await _rabbitMqClient.StartAsync(AppSettings.Get("rabbitmq:routingKey"));
                }
                else
                {
                    _logger.LogError("Failed to start RabbitMQ consumer");
                }
            }

            // Stop program from exiting
            await Task.Delay(Timeout.Infinite);
        }

        private Task OnClientLog(LogMessage logMessage)
        {
            _logger.LogInformation(logMessage.ToString());
            return Task.CompletedTask;
        }

        private async Task OnClientReadyAsync()
        {
            await _discordClient.SetGameAsync("over servers 👀", null, ActivityType.Watching);
        }

        /// <summary>
        /// Only runs if program is started with <see cref="REGISTER_COMMANDS_ARG"/> param
        /// </summary>
        /// <returns></returns>
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

            SocketGuild guild = _discordClient.GetGuild(ulong.Parse(guildId));

            if (guild is null)
            {
                _logger.LogError("Failed to get guild from client, exiting");
                return;
            }

            List<ApplicationCommandProperties> commandsToRegister = SlashCommandsBuilder.GetCommandList();

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

        private static void PrintHelp()
        {
            StringBuilder b = new StringBuilder()
                .AppendLine("Available parameters:")
                .AppendLine("-h | --help")
                .AppendLine("\tPrints this message")
                .AppendLine()
                .AppendLine("--register-commands")
                .AppendLine("\tOverrides bot command list in guild specified in appsettings.json 'settings:discord:guildId'")
                .AppendLine()
                .AppendLine("--rabbitmq")
                .AppendLine("\tEnable connection to RabbitMQ for event consumer")
                .AppendLine("\tMake sure the 'settings:rabbitmq' section exists in your appsettings.json. See appsettings.example.json");

            Console.WriteLine(b);
        }
    }
}
