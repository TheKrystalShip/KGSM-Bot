using Discord.Interactions;

using TheKrystalShip.KGSM.Services;

using TheKrystalShip.Logging;

namespace TheKrystalShip.KGSM.Modules;

// Interaction modules must be public and inherit from an IInteractionModuleBase
public partial class BlueprintsModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly KgsmInterop _interop;
    private readonly DiscordChannelRegistry _discordChannelRegistry;
    private readonly Logger<BlueprintsModule> _logger;

    // Constructor injection is also a valid way to access the dependencies
    public BlueprintsModule(DiscordChannelRegistry discordChannelRegistry, KgsmInterop interop)
    {
        _discordChannelRegistry = discordChannelRegistry;
        _interop = interop;
        _logger = new();
    }

    [SlashCommand("install", "Install a new game server")]
    public async Task InstallAsync(
        [Summary(description: "Blueprint to create a new install from")]
        [Autocomplete(typeof(BlueprintAutocompleteHandler))]
        string blueprint,
        [Summary(description: "Custom installation name")]
        string? customName = null
    )
    {
        await RespondAsync($"Installing {blueprint}...");

        KgsmResult result = _interop.Install(
                blueprintName: blueprint,
                installDir: null,
                version: null,
                name: customName
            );

        if (result.ExitCode == 0) {
            string message = "Installation successful";

            if (result.Stdout != string.Empty)
                message = result.Stdout;
            if (result.Stderr != string.Empty)
                message = result.Stderr;

            await FollowupAsync(message);
        }
        else
        { 
            await FollowupAsync($"Something went wrong.\nExit code {result.ExitCode}\nStdout: {result.Stdout}\nStderr: {result.Stderr}");
        }
    }
}
