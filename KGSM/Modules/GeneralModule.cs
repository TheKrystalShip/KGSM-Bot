using Discord.Interactions;

using TheKrystalShip.Logging;

namespace TheKrystalShip.KGSM.Modules;

// Interaction modules must be public and inherit from an IInteractionModuleBase
public partial class GeneralModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly KgsmInterop _interop;
    private readonly Logger<GeneralModule> _logger;

    // Constructor injection is also a valid way to access the dependencies
    public GeneralModule(KgsmInterop interop)
    {
        _interop = interop;
        _logger = new();
    }

    [SlashCommand("get-ip", "Get the server's IP address")]
    public async Task GetIPAsync()
    {
        KgsmResult result = _interop.GetIp();
        await RespondAsync(result.Stdout ?? result.Stderr);
    }
}
