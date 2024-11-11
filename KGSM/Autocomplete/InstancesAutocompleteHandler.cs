using Discord;
using Discord.Interactions;

using TheKrystalShip.Logging;

namespace TheKrystalShip.KGSM;

public class InstancesAutocompleteHandler : AutocompleteHandler
{
    private List<string> _instances = [];
    private Logger<InstancesAutocompleteHandler> _logger;

    public InstancesAutocompleteHandler()
    {
        _logger = new();
    }

    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        var _interop = services.GetService(typeof(KgsmInterop)) as KgsmInterop ??
            throw new ArgumentNullException("Required KgsmInterop service not found");

        if (_interop is null)
        {
            string errorMessage = "Required service KgsmInterop not found";
            _logger.LogError(errorMessage);
            return AutocompletionResult.FromError(new Exception(errorMessage));
        }

        KgsmResult result = _interop.GetInstances();
        
        if (result.Stdout?.Trim() != string.Empty)
            _instances = result.Stdout?.Split('\n').ToList() ?? [];

        // Parameterless AutocompletionResult.FromSuccess() will display
        // "No options match your search." to the user
        if (_instances.Count == 0)
            return AutocompletionResult.FromSuccess();
        
        string currentInput = autocompleteInteraction.Data.Current.Value?.ToString() ?? string.Empty;
        
        // Generate list of best matches
        List<AutocompleteResult> suggestions = _instances
            .Where(choice => choice.StartsWith(currentInput, StringComparison.OrdinalIgnoreCase))
            .Select(choice => new AutocompleteResult(choice, choice))
            .ToList();

        await Task.CompletedTask;

        // Max 25 suggestions at a time because of Discord API
        return AutocompletionResult.FromSuccess(suggestions.Take(25));
    }
}
