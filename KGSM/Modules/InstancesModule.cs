using Discord;
using Discord.Interactions;

using Game = TheKrystalShip.KGSM.Domain.Game;

using TheKrystalShip.Logging;

namespace TheKrystalShip.KGSM.Modules;

// Interaction modules must be public and inherit from an IInteractionModuleBase
public partial class InstancesModule : InteractionModuleBase<SocketInteractionContext>
{

    private readonly KgsmInterop _interop;
    private readonly Logger<InstancesModule> _logger;

    // Constructor injection is also a valid way to access the dependencies
    public InstancesModule(KgsmInterop interop)
    {
        _interop = interop;
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
        _interop.Start(game.internalName);
    }

    [SlashCommand("stop", "Shut down a game server")]
    public async Task StopAsync(
        [Summary(description: "Text channel of the game server")]
        [ChannelTypes(ChannelType.Text)]
        Game game
    )
    {
        await RespondAsync($"Stopping {game}...");
        _interop.Stop(game.internalName);
    }

    [SlashCommand("restart", "Restart a game server")]
    public async Task RestartAsync(
        [Summary(description: "Text channel of the game server")]
        [ChannelTypes(ChannelType.Text)]
        Game game
    )
    {
        await RespondAsync($"Restarting {game}...");
        _interop.Restart(game.internalName);
    }

    [SlashCommand("status", "Get a detailed status of a game server")]
    public async Task StatusAsync(
        [Summary(description: "Text channel of the game server")]
        [ChannelTypes(ChannelType.Text)]
        Game game
    )
    {
        KgsmResult result = _interop.Status(game.internalName);
        await RespondAsync(result.Stdout ?? result.Stderr);
    }

    [SlashCommand("info", "Get information about the game server installation")]
    public async Task InfoAsync(
        [Summary(description: "Text channel of the game server")]
        [ChannelTypes(ChannelType.Text)]
        Game game
    )
    {
        KgsmResult result = _interop.Info(game.internalName);
        await RespondAsync(result.Stdout ?? result.Stderr);
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
                _ => "invalid state"
            };

        KgsmResult result = _interop.IsActive(game.internalName);
        string outputMessage = $"{game} is {GetSynonym(result.Stdout ?? result.Stderr ?? string.Empty)}";

        await RespondAsync(outputMessage);
    }

    [SlashCommand("get-logs", "Get the last 10 lines a game server log")]
    public async Task GetLogsAsync(
        [Summary(description: "Text channel of the game server")]
        [ChannelTypes(ChannelType.Text)]
        Game game
    )
    {
        await RespondAsync($"Fetching logs...");

        KgsmResult result = _interop.GetLogs(game.internalName);

        string followupText = $"No logs found";
        if (result.ExitCode == 0)
        {
            followupText = result.Stdout ?? string.Empty;
        }

        EmbedBuilder embedBuilder = new EmbedBuilder()
            .WithAuthor(Context.User.ToString(), Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
            .WithTitle($"{game.displayName} logs:")
            .WithDescription(followupText)
            .WithColor(Color.Green)
            .WithCurrentTimestamp();

        await FollowupAsync(embed: embedBuilder.Build());
    }

    [SlashCommand("uninstall", "Uninstall a game server")]
    public async Task UninstallAsync(
        [Summary(description: "Game server instance")]
        [Autocomplete(typeof(InstancesAutocompleteHandler))]
        string instance
    )
    {
        await RespondAsync($"Uninstalling {instance}...");

        KgsmResult result = _interop.Uninstall(instance);

        if (result.ExitCode == 0) {
            string message = "Uninstall successful";

            if (result.Stdout != string.Empty)
                message = result.Stdout;
            if (result.Stderr != string.Empty)
                message = result.Stderr;
            
            await FollowupAsync(message);
        } else {
            await FollowupAsync($"Something went wrong.\nExit code {result.ExitCode}\nStdout: {result.Stdout}\nStderr: {result.Stderr}");
        }
    }
}
