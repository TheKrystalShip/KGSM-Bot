using Discord;
using Discord.Interactions;

using Game = TheKrystalShip.KGSM.Domain.Game;

using TheKrystalShip.Logging;

namespace TheKrystalShip.KGSM.Modules;

// Interaction modules must be public and inherit from an IInteractionModuleBase
public partial class ServerModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly KgsmInterop _interop;
    private readonly Logger<ServerModule> _logger;

    // Constructor injection is also a valid way to access the dependencies
    public ServerModule(KgsmInterop interop)
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

    [SlashCommand("get-ip", "Get the server's IP address")]
    public async Task GetIPAsync()
    {
        KgsmResult result = _interop.GetIp();
        await RespondAsync(result.Stdout ?? result.Stderr);
    }

    [SlashCommand("install", "Install a new game server")]
    public async Task InstallAsync(
        [Summary(description: "Blueprint to create a new install from")]
        [Autocomplete(typeof(BlueprintAutocompleteHandler))]
        string blueprint
    )
    {
        await RespondAsync($"Installing {blueprint}...");

        KgsmResult result = _interop.Install(blueprint);
        await FollowupAsync(result.Stderr ?? result.Stdout);
    }

    public class BlueprintAutocompleteHandler : AutocompleteHandler
    {
        private readonly List<string> _blueprints = [];
        private readonly KgsmInterop _interop;

        public BlueprintAutocompleteHandler(KgsmInterop interop)
        {
            _interop = interop;
            KgsmResult result = _interop.GetBlueprints();

            if (result.ExitCode == 0) {
                _blueprints = result.Stdout?.Split('\n').ToList() ?? [];
            }
        }

        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            string currentInput = autocompleteInteraction.Data.Current.Value?.ToString() ?? string.Empty;
            
            // Generate list of best matches
            IEnumerable<AutocompleteResult> suggestions = _blueprints
                .Where(choice => choice.StartsWith(currentInput, StringComparison.OrdinalIgnoreCase))
                .Select(choice => new AutocompleteResult(choice, choice))
                .ToList();

            // Max 25 suggestions at a time because of Discord API
            return AutocompletionResult.FromSuccess(suggestions.Take(25));
        }
    }
}
