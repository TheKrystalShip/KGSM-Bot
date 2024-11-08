using Discord;
using Discord.Interactions;

using Game = TheKrystalShip.KGSM.Domain.Game;

using TheKrystalShip.Logging;

namespace TheKrystalShip.KGSM.Modules;

// Interaction modules must be public and inherit from an IInteractionModuleBase
public partial class BlueprintsModule : InteractionModuleBase<SocketInteractionContext>
{

    private readonly KgsmInterop _interop;
    private readonly Logger<BlueprintsModule> _logger;

    // Constructor injection is also a valid way to access the dependencies
    public BlueprintsModule(KgsmInterop interop)
    {
        _interop = interop;
        _logger = new();
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
}
