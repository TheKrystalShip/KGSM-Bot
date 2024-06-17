using Discord;
using Discord.Interactions;

using Microsoft.Extensions.Configuration;

using TheKrystalShip.Admiral.Domain;
using Game = TheKrystalShip.Admiral.Domain.Game;

using TheKrystalShip.Logging;

namespace TheKrystalShip.Admiral.Modules;

// Interaction modules must be public and inherit from an IInteractionModuleBase
public partial class ServerModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly InteractionHandler _handler;
    private readonly ICommandExecutioner _executioner;
    private readonly IConfiguration _configuration;

    private readonly Logger<ServerModule> _logger;

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

    [SlashCommand("start", "Start up a game server")]
    public async Task StartAsync(
        [Summary(description: "Text channel of the game server")]
        [ChannelTypes(ChannelType.Text)]
        Game game
    )
    {
        await RespondAsync($"Starting {game}...");
        _executioner.Start(game.internalName);
    }

    [SlashCommand("stop", "Shut down a game server")]
    public async Task StopAsync(
        [Summary(description: "Text channel of the game server")]
        [ChannelTypes(ChannelType.Text)]
        Game game
    )
    {
        await RespondAsync($"Stopping {game}...");
        _executioner.Stop(game.internalName);
    }

    [SlashCommand("restart", "Restart a game server")]
    public async Task RestartAsync(
        [Summary(description: "Text channel of the game server")]
        [ChannelTypes(ChannelType.Text)]
        Game game
    )
    {
        await RespondAsync($"Restarting {game}...");
        _executioner.Restart(game.internalName);
    }

    [SlashCommand("status", "Get a detailed status of a game server")]
    public async Task StatusAsync(
        [Summary(description: "Text channel of the game server")]
        [ChannelTypes(ChannelType.Text)]
        Game game
    )
    {
        Result result = _executioner.Status(game.internalName);
        await RespondAsync(result.Output);
    }

    [SlashCommand("is-active", "Check if a game server is currently running")]
    public async Task IsActiveAsync(
        [Summary(description: "Text channel of the game server")]
        [ChannelTypes(ChannelType.Text)]
        Game game
    )
    {
        static string GetSynonym(string input) =>
            input switch
            {
                "active" => "online",
                "inactive" => "offline",
                _ => input
            };

        Result result = _executioner.IsActive(game.internalName);
        result.Output = $"{game} is {GetSynonym(result.Output)}";

        await RespondAsync(result.Output);
    }

    [SlashCommand("get-logs", "Get the last 10 lines a game server log")]
    public async Task GetLogsAsync(
        [Summary(description: "Text channel of the game server")]
        [ChannelTypes(ChannelType.Text)]
        Game game
    )
    {
        await RespondAsync($"Fetching logs...");

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
        Result result = _executioner.GetIp();
        await RespondAsync(result.Output);
    }
}
