using Discord;
using Discord.Interactions;

using System.Text.RegularExpressions;

using Game = TheKrystalShip.KGSM.Domain.Game;

using TheKrystalShip.KGSM.Services;

using TheKrystalShip.Logging;

namespace TheKrystalShip.KGSM.Modules;

// Interaction modules must be public and inherit from an IInteractionModuleBase
public partial class BlueprintsModule : InteractionModuleBase<SocketInteractionContext>
{

    private readonly KgsmInterop _interop;
    private readonly DiscordNotifier _discordNotifier;
    private readonly Logger<BlueprintsModule> _logger;

    // Constructor injection is also a valid way to access the dependencies
    public BlueprintsModule(DiscordNotifier discordNotifier, KgsmInterop interop)
    {
        _discordNotifier = discordNotifier;
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
                id: customName
            );

        if (result.ExitCode == 0) {
            string message = "Uninstall successful";

            if (result.Stdout != string.Empty)
                message = result.Stdout;
            if (result.Stderr != string.Empty)
                message = result.Stderr;
            string pattern = @"Instance\s+(.+?)\s+has";
            Match match = Regex.Match(message, pattern);

            if (match.Success) {
                string instanceId = match.Groups[1].Value;
                await _discordNotifier.OnInstanceInstalledAsync(Context.Guild.Id, instanceId);
            }

            await FollowupAsync(result.Stderr ?? result.Stdout);
        }
        else
        { 
            await FollowupAsync($"Something went wrong.\nExit code {result.ExitCode}\nStdout: {result.Stdout}\nStderr: {result.Stderr}");
        }
    }
}
