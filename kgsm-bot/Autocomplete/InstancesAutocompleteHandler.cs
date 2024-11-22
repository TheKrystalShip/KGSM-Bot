﻿using Discord;
using Discord.Interactions;

namespace TheKrystalShip.KGSM;

public class InstancesAutocompleteHandler : AutocompleteHandler
{
    private List<string> _instances = [];

    public InstancesAutocompleteHandler(KgsmInterop interop)
    {
        KgsmResult result = interop.GetInstances();
        
        if (result.Stdout?.Trim() != string.Empty)
            _instances = result.Stdout?.Split('\n').ToList() ?? [];
    }

    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
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