using Discord;
using Discord.Interactions;

using Microsoft.Extensions.Configuration;

using TheKrystalShip.Admiral.Domain;
using Game = TheKrystalShip.Admiral.Domain.Game;

using TheKrystalShip.Logging;

using System.Text.RegularExpressions;

namespace TheKrystalShip.Admiral.Modules;

// Interaction modules must be public and inherit from an IInteractionModuleBase
public partial class ServerModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly InteractionHandler _handler;
    private readonly ICommandExecutioner _executioner;
    private readonly IConfiguration _configuration;

    private readonly Logger<ServerModule> _logger;

    [GeneratedRegex("[^a-zA-Z0-9_.]+", RegexOptions.Compiled)]
    private static partial Regex ChannelNameRegex();

    // Constructor injection is also a valid way to access the dependencies
    public ServerModule(
        InteractionHandler handler,
        ICommandExecutioner commandExecutioner,
        IConfiguration configuration
    )
    {
        _handler = handler;
        _executioner = commandExecutioner;
        _configuration = configuration;
        _logger = new();
    }

    // You can use a number of parameter types in you Slash Command handlers
    // (string, int, double, bool, IUser, IChannel, IMentionable, IRole, Enums)
    // by default.
    // Optionally, you can implement your own TypeConverters to support a wider
    // range of parameter types. For more information, refer to the library
    // documentation.
    //
    // Optional method parameters(parameters with a default value) also will be
    // displayed as optional on Discord.

    [SlashCommand("start", "Start up a game server")]
    public async Task StartAsync(
        [Summary(description: "Text channel of the game server")]
        [ChannelTypes(ChannelType.Text)]
        IChannel channel
    )
    {
        string serviceName = ChannelNameRegex()
            .Replace(channel.Name, string.Empty);

        Game game = new(
            internalName: serviceName,
            displayName: _configuration[$"games:{serviceName}:displayName"] ?? string.Empty,
            channelId: _configuration[$"games:{serviceName}:channelId"] ?? string.Empty
        );

        await RespondAsync($"Starting {game}...");
        _executioner.Start(game.internalName);
    }

    [SlashCommand("stop", "Shut down a game server")]
    public async Task StopAsync(
        [Summary(description: "Text channel of the game server")]
        [ChannelTypes(ChannelType.Text)]
        IChannel channel
    )
    {
        string serviceName = ChannelNameRegex()
            .Replace(channel.Name, string.Empty);

        Game game = new(
            internalName: serviceName,
            displayName: _configuration[$"games:{serviceName}:displayName"] ?? string.Empty,
            channelId: _configuration[$"games:{serviceName}:channelId"] ?? string.Empty
        );

        await RespondAsync($"Stopping {game}...");
        _executioner.Stop(game.internalName);
    }

    [SlashCommand("restart", "Restart a game server")]
    public async Task RestartAsync(
        [Summary(description: "Text channel of the game server")]
        [ChannelTypes(ChannelType.Text)]
        IChannel channel
    )
    {
        string serviceName = ChannelNameRegex()
            .Replace(channel.Name, string.Empty);

        Game game = new(
            internalName: serviceName,
            displayName: _configuration[$"games:{serviceName}:displayName"] ?? string.Empty,
            channelId: _configuration[$"games:{serviceName}:channelId"] ?? string.Empty
        );

        await RespondAsync($"Restarting {game}...");
        _executioner.Restart(game.internalName);
    }

    [SlashCommand("status", "Get a detailed status of a game server")]
    public async Task StatusAsync(
        [Summary(description: "Text channel of the game server")]
        [ChannelTypes(ChannelType.Text)]
        IChannel channel
    )
    {
        string serviceName = ChannelNameRegex()
            .Replace(channel.Name, string.Empty);

        Game game = new(
            internalName: serviceName,
            displayName: _configuration[$"games:{serviceName}:displayName"] ?? string.Empty,
            channelId: _configuration[$"games:{serviceName}:channelId"] ?? string.Empty
        );

        Result result = _executioner.Status(game.internalName);
        await RespondAsync(result.Output);
    }

    [SlashCommand("is-active", "Check if a game server is currently running")]
    public async Task IsActiveAsync(
        [Summary(description: "Text channel of the game server")]
        [ChannelTypes(ChannelType.Text)]
        IChannel channel
    )
    {
        string serviceName = ChannelNameRegex()
            .Replace(channel.Name, string.Empty);

        Game game = new(
            internalName: serviceName,
            displayName: _configuration[$"games:{serviceName}:displayName"] ?? string.Empty,
            channelId: _configuration[$"games:{serviceName}:channelId"] ?? string.Empty
        );

        static string GetSynonym(string input) =>
            input switch
            {
                "active" => "online",
                "inactive" => "offline",
                _ => input
            };

        var isActiveResult = _executioner.IsActive(game.internalName);
        string result = $"{game} is {GetSynonym(isActiveResult.Output)}";

        await RespondAsync(result);
    }

    [SlashCommand("get-logs", "Get the last 10 lines a game server log")]
    public async Task GetLogsAsync(
        [Summary(description: "Text channel of the game server")]
        [ChannelTypes(ChannelType.Text)]
        IChannel channel
    )
    {
        await RespondAsync($"Fetching logs...");

        string serviceName = ChannelNameRegex()
            .Replace(channel.Name, string.Empty);

        Game game = new(
            internalName: serviceName,
            displayName: _configuration[$"games:{serviceName}:displayName"] ?? string.Empty,
            channelId: _configuration[$"games:{serviceName}:channelId"] ?? string.Empty
        );

        Result result = _executioner.GetLogs(game.internalName);

        string followupText = $"No logs found";
        if (result.IsSuccessWithOutput)
        {
            followupText = result.Output;
        }

        EmbedBuilder embedBuilder = new EmbedBuilder()
            .WithAuthor(Context.User.ToString(), Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
            .WithTitle($"{game.displayName} logs:")
            .WithDescription(followupText)
            .WithColor(Color.Green)
            .WithCurrentTimestamp();

        await FollowupAsync(embed: embedBuilder.Build());
    }

    [SlashCommand("get-ip", "Get the server's IP address")]
    public async Task GetIPAsync()
    {
        var result = _executioner.GetIp();
        await RespondAsync(result.Output);
    }
}
