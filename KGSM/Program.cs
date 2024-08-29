using Discord;
using Discord.Interactions;
using Discord.WebSocket;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using TheKrystalShip.KGSM.Services;
using TheKrystalShip.Logging;

namespace TheKrystalShip.KGSM;

public class Program
{
    private static bool _useRabbitMq = false;

    private IConfiguration _configuration;
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
        if (args.Contains("--rabbitmq"))
            _useRabbitMq = true;

        _configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        string? kgsmPath = _configuration["kgsmPath"] ??
            throw new Exception("\"kgsmPath\" not set in appsettings.json");

        var serviceCollection = new ServiceCollection()
            .AddSingleton(_configuration)
            .AddSingleton(_socketConfig)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton(x => new InteractionService(
                x.GetRequiredService<DiscordSocketClient>(),
                _interactionServiceConfig
            ))
            .AddSingleton<InteractionHandler>()
            .AddSingleton<GameTypeConverter>()
            .AddSingleton<DiscordNotifier>()
            .AddSingleton(x => new KgsmInterop(kgsmPath));

        if (_useRabbitMq)
        {
            _logger.LogInformation("Using RabbitMQ");
            serviceCollection.AddSingleton(
                new RabbitMQClient(_configuration["rabbitmq:uri"] ?? string.Empty)
            );
        }

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

        await discordClient.LoginAsync(TokenType.Bot, _configuration["discord:token"]);
        await discordClient.StartAsync();

        if (_useRabbitMq)
        {
            var rabbitMqClient = _services.GetRequiredService<RabbitMQClient>();
            var discordNotifier = _services.GetRequiredService<DiscordNotifier>();
            rabbitMqClient.StatusChangeReceived += discordNotifier.OnRunningStatusUpdated;

            if (await rabbitMqClient.LoginAsync())
            {
                await rabbitMqClient.StartAsync(
                    _configuration["rabbitmq:routingKey"] ?? string.Empty
                );
            }
        }

        // Never quit the program until manually forced to.
        await Task.Delay(Timeout.Infinite);
    }
}
