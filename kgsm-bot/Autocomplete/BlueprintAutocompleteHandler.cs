using Discord;
using Discord.Interactions;

using TheKrystalShip.KGSM.Lib;

namespace TheKrystalShip.KGSM;

public class BlueprintAutocompleteHandler : AutocompleteHandler
{
    private readonly List<string> _blueprints = [];

    public BlueprintAutocompleteHandler(KgsmInterop interop)
    {
        Dictionary<string, Blueprint> result = interop.GetBlueprints();
        _blueprints = result.Keys.ToList();
    }

    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        string currentInput = autocompleteInteraction.Data.Current.Value?.ToString() ?? string.Empty;
        
        // Generate list of best matches
        IEnumerable<AutocompleteResult> suggestions = _blueprints
            .Where(choice => choice.StartsWith(currentInput, StringComparison.OrdinalIgnoreCase))
            .Select(choice => new AutocompleteResult(choice, choice))
            .ToList();

        await Task.CompletedTask;

        // Max 25 suggestions at a time because of Discord API
        return AutocompletionResult.FromSuccess(suggestions.Take(25));
    }
}