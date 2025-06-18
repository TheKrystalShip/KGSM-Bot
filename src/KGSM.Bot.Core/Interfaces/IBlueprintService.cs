using KGSM.Bot.Core.Common;

using TheKrystalShip.KGSM.Core.Models;

namespace KGSM.Bot.Core.Interfaces;

/// <summary>
/// Interface for managing game server blueprints
/// </summary>
public interface IBlueprintService
{
    /// <summary>
    /// Gets all available blueprints
    /// </summary>
    /// <returns>Collection of blueprints</returns>
    Task<Result<IReadOnlyDictionary<string, Blueprint>>> GetAllAsync();

    /// <summary>
    /// Gets a blueprint by name
    /// </summary>
    /// <param name="blueprintName">Name of the blueprint</param>
    /// <returns>The blueprint, if found</returns>
    Task<Result<Blueprint>> GetByNameAsync(string blueprintName);

    /// <summary>
    /// Creates a new blueprint
    /// </summary>
    /// <param name="blueprint">The blueprint to create</param>
    /// <returns>Result of the operation</returns>
    Task<Result> CreateAsync(Blueprint blueprint);
}
