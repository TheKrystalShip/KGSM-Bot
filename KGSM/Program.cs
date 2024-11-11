using Discord;
using Discord.Interactions;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using TheKrystalShip.KGSM.Services;
using TheKrystalShip.Logging;

namespace TheKrystalShip.KGSM;

public class Program
{
    private AppSettingsManager _settingsManager;
    private IServiceProvider _services;

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

    public static void Main(string[] args) =>
        new Program().RunAsync(args).GetAwaiter().GetResult();

    public async Task RunAsync(string[] args)
    {
        _settingsManager = new("appsettings.json");

        string kgsmPath = _settingsManager.Settings.KgsmPath;

        if (kgsmPath == string.Empty)
            throw new Exception("\"kgsmPath\" not set in appsettings.json");

        var serviceCollection = new ServiceCollection()
            .AddSingleton(_socketConfig)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton(x => new InteractionService(
                x.GetRequiredService<DiscordSocketClient>(),
                _interactionServiceConfig
            ))
            .AddSingleton<InteractionHandler>()
            .AddSingleton<DiscordNotifier>()
            .AddSingleton<DiscordChannelRegistry>()
            .AddSingleton<WatchdogNotifier>()
            .AddSingleton(x => new KgsmInterop(kgsmPath))
            .AddSingleton(x => new AppSettingsManager("appsettings.json"));

        _services = serviceCollection.BuildServiceProvider();

        // Here we can initialize the service that will register and execute our commands
        await _services.GetRequiredService<InteractionHandler>()
            .InitializeAsync();

        var discordClient = _services.GetRequiredService<DiscordSocketClient>();
        discordClient.Log += (logMessage) =>
        {
            _logger.LogInformation(logMessage.ToString());
            return Task.CompletedTask;
        };
        discordClient.Ready += async () =>
        {
            await discordClient.SetGameAsync("over servers 👀", null, ActivityType.Watching);
        };

        await discordClient.LoginAsync(TokenType.Bot, _settingsManager.Settings.Discord.Token);
        await discordClient.StartAsync();

        // Never quit the program until manually forced to.
        await Task.Delay(Timeout.Infinite);
    }
}
