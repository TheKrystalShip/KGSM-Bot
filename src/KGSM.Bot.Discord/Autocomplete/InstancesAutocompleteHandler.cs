using Discord;
using Discord.Interactions;

using KGSM.Bot.Application.Queries;

using MediatR;

using Microsoft.Extensions.Logging;

namespace KGSM.Bot.Discord.Autocomplete;

/// <summary>
/// Autocomplete handler for instances
/// </summary>
public class InstancesAutocompleteHandler : AutocompleteHandler
{
    private readonly IMediator _mediator;
    private readonly ILogger<InstancesAutocompleteHandler> _logger;

    public InstancesAutocompleteHandler(
        IMediator mediator,
        ILogger<InstancesAutocompleteHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        try
        {
            _logger.LogDebug("Generating instance suggestions for autocomplete");

            // Get current value
            string currentValue = autocompleteInteraction.Data.Current.Value.ToString() ?? string.Empty;

            // Get all instances
            var result = await _mediator.Send(new GetAllInstancesQuery());
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to get instances for autocomplete: {Error}", result.ErrorMessage);
                return AutocompletionResult.FromError(new Exception(result.ErrorMessage));
            }

            // Filter instances by current value
            var filteredInstances = result.Instances!
                .Where(i => i.Key.Contains(currentValue, StringComparison.OrdinalIgnoreCase))
                .Select(i => new AutocompleteResult(i.Key, i.Key))
                .Take(25) // Discord has a limit of 25 autocomplete results
                .ToList();

            _logger.LogDebug("Generated {Count} instance suggestions for autocomplete", filteredInstances.Count);
            return AutocompletionResult.FromSuccess(filteredInstances);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating instance suggestions for autocomplete");
            return AutocompletionResult.FromError(ex);
        }
    }
}
