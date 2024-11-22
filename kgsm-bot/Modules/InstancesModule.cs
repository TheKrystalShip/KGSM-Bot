using Discord;
using Discord.Interactions;

using TheKrystalShip.KGSM.Services;

using TheKrystalShip.Logging;

namespace TheKrystalShip.KGSM.Modules;

// Interaction modules must be public and inherit from an IInteractionModuleBase
public partial class InstancesModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly DiscordChannelRegistry _discordChannelRegistry;
    private readonly KgsmInterop _interop;
    private readonly Logger<InstancesModule> _logger;

    private const string SUMMARY = "Game server instance";

    // Constructor injection is also a valid way to access the dependencies
    public InstancesModule(DiscordChannelRegistry discordChannelRegistry, KgsmInterop interop)
    {
        _discordChannelRegistry = discordChannelRegistry;
        _interop = interop;
        _logger = new();
    }

    [SlashCommand("start", "Start up a game server")]
    public async Task StartAsync(
        [Summary(description: SUMMARY)]
        [Autocomplete(typeof(InstancesAutocompleteHandler))]
        string instance
    )
    {
        await RespondAsync($"Starting {instance}...");
        _interop.Start(instance);
    }

    [SlashCommand("stop", "Shut down a game server")]
    public async Task StopAsync(
        [Summary(description: SUMMARY)]
        [Autocomplete(typeof(InstancesAutocompleteHandler))]
        string instance
    )
    {
        await RespondAsync($"Stopping {instance}...");
        _interop.Stop(instance);
    }

    [SlashCommand("restart", "Restart a game server")]
    public async Task RestartAsync(
        [Summary(description: SUMMARY)]
        [Autocomplete(typeof(InstancesAutocompleteHandler))]
        string instance
    )
    {
        await RespondAsync($"Restarting {instance}...");
        _interop.Restart(instance);
    }

    [SlashCommand("status", "Get a detailed status of a game server")]
    public async Task StatusAsync(
        [Summary(description: SUMMARY)]
        [Autocomplete(typeof(InstancesAutocompleteHandler))]
        string instance
    )
    {
        KgsmResult result = _interop.Status(instance);
        await RespondAsync(result.Stdout ?? result.Stderr);
    }

    [SlashCommand("info", "Get information about the game server installation")]
    public async Task InfoAsync(
        [Summary(description: SUMMARY)]
        [Autocomplete(typeof(InstancesAutocompleteHandler))]
        string instance
    )
    {
        KgsmResult result = _interop.Info(instance);
        await RespondAsync(result.Stdout ?? result.Stderr);
    }

    [SlashCommand("is-active", "Check if an instance is currently running")]
    public async Task IsActiveAsync(
        [Summary(description: SUMMARY)]
        [Autocomplete(typeof(InstancesAutocompleteHandler))]
        string instance
    )
    {
        static string GetSynonym(string input) =>
            input switch
            {
                "active" => "online",
                "inactive" => "offline",
                _ => "invalid state"
            };

        KgsmResult result = _interop.IsActive(instance);
        string outputMessage = $"{instance} is {GetSynonym(result.Stdout ?? result.Stderr ?? string.Empty)}";

        await RespondAsync(outputMessage);
    }

    [SlashCommand("get-logs", "Get the last 10 lines a game server log")]
    public async Task GetLogsAsync(
        [Summary(description: SUMMARY)]
        [Autocomplete(typeof(InstancesAutocompleteHandler))]
        string instance
    )
    {
        await RespondAsync($"Fetching logs for {instance}...");

        KgsmResult result = _interop.GetLogs(instance);

        string followupText = $"No logs found";
        if (result.ExitCode == 0)
        {
            followupText = result.Stdout ?? string.Empty;
        }

        EmbedBuilder embedBuilder = new EmbedBuilder()
            .WithAuthor(Context.User.ToString(), Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
            .WithTitle($"{instance} logs:")
            .WithDescription(followupText)
            .WithColor(Color.Green)
            .WithCurrentTimestamp();

        await FollowupAsync(embed: embedBuilder.Build());
    }

    [SlashCommand("input", "Send a command to the game server instance")]
    public async Task InputAsync(
        [Summary(description: SUMMARY)]
        [Autocomplete(typeof(InstancesAutocompleteHandler))]
        string instance,
        [Summary(description: "Command to send to the server")]
        string command
    )
    {
        KgsmResult result = _interop.AdHoc("-i", instance, "--input", $"'{command}'");
        await Task.CompletedTask;
    }

    [SlashCommand("uninstall", "Uninstall a game server")]
    public async Task UninstallAsync(
        [Summary(description: SUMMARY)]
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
