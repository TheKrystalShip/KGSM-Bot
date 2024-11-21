using Discord;
using Discord.Interactions;
using Discord.WebSocket;

using System.Linq;

using Microsoft.Extensions.DependencyInjection;

using TheKrystalShip.KGSM.Services;
using TheKrystalShip.Logging;

namespace TheKrystalShip.KGSM;

public class KgsmBotStartup
{
    private const string APP_SETTINGS_FILE = "appsettings.json";
    private readonly Logger<Program> _logger = new();

    private readonly DiscordSocketConfig _socketConfig = new()
    {
        GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages
    };

    private readonly InteractionServiceConfig _interactionServiceConfig = new()
    {
        DefaultRunMode = RunMode.Async,
        LogLevel = LogSeverity.Debug,
        ThrowOnError = true
    };

    public KgsmBotStartup()
    {
    }

    public async Task RunAsync(string[] args)
    {
        AppSettingsManager settingsManager = new(APP_SETTINGS_FILE);

        var serviceCollection = new ServiceCollection()
            .AddSingleton(_socketConfig)
            .AddSingleton(settingsManager)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton(x => new InteractionService(
                x.GetRequiredService<DiscordSocketClient>(),
                _interactionServiceConfig
            ))
            .AddSingleton<InteractionHandler>()
            .AddSingleton<DiscordNotifier>()
            .AddSingleton<DiscordChannelRegistry>()
            .AddSingleton<WatchdogNotifier>()
            .AddSingleton<KgsmEventListener>()
            .AddSingleton(x => new KgsmInterop(
                settingsManager.Settings.Kgsm.Path,
                settingsManager.Settings.Kgsm.SocketPath
            ));

        IServiceProvider services = serviceCollection.BuildServiceProvider();

        // Here we can initialize the service that will register and execute our commands
        await services.GetRequiredService<InteractionHandler>()
            .InitializeAsync();

        var discordClient = services.GetRequiredService<DiscordSocketClient>();
        
        discordClient.Log += (logMessage) =>
        {
            _logger.LogInformation(logMessage.ToString());
            return Task.CompletedTask;
        };

        discordClient.Ready += async () =>
        {
            await discordClient.SetGameAsync("over servers 👀", null, ActivityType.Watching);
        
            services.GetRequiredService<KgsmEventListener>()
                .Initialize();
        };

        await discordClient.LoginAsync(TokenType.Bot, settingsManager.Settings.Discord.Token);
        await discordClient.StartAsync();

        // Never quit the program until manually forced to.
        await Task.Delay(Timeout.Infinite);
    }
}
