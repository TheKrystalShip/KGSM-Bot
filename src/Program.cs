using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using TheKrystalShip.Admiral.Services;
using TheKrystalShip.Admiral.Tools;

namespace TheKrystalShip.Admiral
{
    public class Program
    {
        private readonly IServiceProvider _services;

        public static void Main(string[] args)
            => new Program().RunAsync().GetAwaiter().GetResult();

        public Program()
        {
            // This verifies the entire appsettings.json file before anything
            // runs, thus no need to check for null every time a value is fetched.
            AppSettingsVerifier.Verify();

            // Register DI services
            _services = CreateServices();
        }

        private static ServiceProvider CreateServices()
        {
            IServiceCollection collection = new ServiceCollection()
                .AddSingleton(new DiscordSocketConfig())
                .AddSingleton(new CommandExecutioner())
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<DiscordCommandHandler>();

            return collection.BuildServiceProvider();
        }

        public async Task RunAsync()
        {
            DiscordSocketClient client = _services.GetRequiredService<DiscordSocketClient>();
            DiscordCommandHandler commandHandler = _services.GetRequiredService<DiscordCommandHandler>();

            client.SlashCommandExecuted += commandHandler.HandleCommand;

            await client.LoginAsync(Discord.TokenType.Bot, AppSettings.Get("discord:token"));
            await client.StartAsync();

            // Stop program from exiting
            await Task.Delay(Timeout.Infinite);
        }
    }
}
