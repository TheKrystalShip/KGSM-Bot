using KGSM.Bot.Core.Common;

namespace KGSM.Bot.Core.Interfaces;

/// <summary>
/// Interface for managing Discord channel registry for game server instances
/// </summary>
public interface IDiscordChannelRegistry
{
    /// <summary>
    /// Adds or updates a Discord channel for a game server instance
    /// </summary>
    /// <param name="guildId">Discord guild (server) ID</param>
    /// <param name="blueprintName">Name of the blueprint</param>
    /// <param name="instanceName">Name of the instance</param>
    /// <returns>Result of the operation</returns>
    Task<Result> AddOrUpdateChannelAsync(ulong guildId, string blueprintName, string instanceName);

    /// <summary>
    /// Removes a Discord channel for a game server instance
    /// </summary>
    /// <param name="guildId">Discord guild (server) ID</param>
    /// <param name="instanceName">Name of the instance</param>
    /// <returns>Result of the operation</returns>
    Task<Result> RemoveChannelAsync(ulong guildId, string instanceName);

    /// <summary>
    /// Gets the Discord channel ID for a game server instance
    /// </summary>
    /// <param name="instanceName">Name of the instance</param>
    /// <returns>Discord channel ID, if found</returns>
    Task<Result<ulong>> GetChannelIdAsync(string instanceName);
}
