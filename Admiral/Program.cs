using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using TheKrystalShip.Admiral.Services;
using TheKrystalShip.Admiral.Tools;
using TheKrystalShip.Logging;

namespace TheKrystalShip.Admiral
{
    public class Program
    {
        private readonly IServiceProvider _services;
        private readonly Logger<Program> _logger;

        public static void Main(string[] args)
            => new Program().RunAsync().GetAwaiter().GetResult();

        public Program()
        {
            _logger = new();
            _services = CreateServices();
        }

        private static ServiceProvider CreateServices()
        {
            IServiceCollection collection = new ServiceCollection()
                .AddSingleton(new DiscordSocketConfig() {
                    // https://discord.com/developers/docs/topics/gateway#gateway-intents
                    GatewayIntents =
                        GatewayIntents.Guilds |
                        GatewayIntents.GuildMessages
                })
                .AddSingleton(new CommandExecutioner())
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<SlashCommandHandler>();

            return collection.BuildServiceProvider();
        }

        public async Task RunAsync()
        {
            DiscordSocketClient client = _services.GetRequiredService<DiscordSocketClient>();
            SlashCommandHandler commandHandler = _services.GetRequiredService<SlashCommandHandler>();

            client.SlashCommandExecuted += commandHandler.HandleCommand;
            client.Log += OnClientLog;
            client.Ready += () => OnClientReady(client);

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

        private async Task OnClientReady(DiscordSocketClient client)
        {
            await client.SetGameAsync("over servers 👀", null, ActivityType.Watching);

            // Leave commented unless you know what you're doing
            // await _services.GetRequiredService<SlashCommandHandler>().RegisterSlashCommands();
        }
    }
}
