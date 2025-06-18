using KGSM.Bot.Core.Common;

using TheKrystalShip.KGSM.Core.Models;

namespace KGSM.Bot.Core.Interfaces;

/// <summary>
/// Interface for managing game server instances
/// </summary>
public interface IServerInstanceService
{
    /// <summary>
    /// Gets all server instances
    /// </summary>
    /// <returns>Collection of server instances</returns>
    Task<Result<IReadOnlyDictionary<string, Instance>>> GetAllAsync();

    /// <summary>
    /// Gets a server instance by name
    /// </summary>
    /// <param name="instanceName">Name of the instance</param>
    /// <returns>The server instance, if found</returns>
    Task<Result<Instance>> GetByNameAsync(string instanceName);

    /// <summary>
    /// Installs a new server instance from a blueprint
    /// </summary>
    /// <param name="blueprintName">Name of the blueprint to use</param>
    /// <param name="instancePath">Installation path for the instance (optional)</param>
    /// <param name="version">Version to install (optional)</param>
    /// <param name="name">Name for the instance (optional)</param>
    /// <returns>Result of the operation</returns>
    Task<Result> InstallAsync(string blueprintName, string? instancePath = null, string? version = null, string? name = null);

    /// <summary>
    /// Uninstalls a server instance
    /// </summary>
    /// <param name="instanceName">Name of the instance to uninstall</param>
    /// <returns>Result of the operation</returns>
    Task<Result> UninstallAsync(string instanceName);

    /// <summary>
    /// Starts a server instance
    /// </summary>
    /// <param name="instanceName">Name of the instance to start</param>
    /// <returns>Result of the operation</returns>
    Task<Result> StartAsync(string instanceName);

    /// <summary>
    /// Stops a server instance
    /// </summary>
    /// <param name="instanceName">Name of the instance to stop</param>
    /// <returns>Result of the operation</returns>
    Task<Result> StopAsync(string instanceName);

    /// <summary>
    /// Restarts a server instance
    /// </summary>
    /// <param name="instanceName">Name of the instance to restart</param>
    /// <returns>Result of the operation</returns>
    Task<Result> RestartAsync(string instanceName);

    /// <summary>
    /// Gets information about a server instance
    /// </summary>
    /// <param name="instanceName">Name of the instance</param>
    /// <returns>Information result</returns>
    Task<Result<string>> GetInfoAsync(string instanceName);

    /// <summary>
    /// Checks if the instance is currently active (running)
    /// </summary>
    /// <param name="instanceName">Name of the instance</param>
    /// <returns>True if active, false otherwise</returns>
    Task<Result<bool>> IsActiveAsync(string instanceName);

    /// <summary>
    /// Creates a backup of the instance
    /// </summary>
    /// <param name="instanceName">Name of the instance</param>
    /// <returns>Result of the operation</returns>
    Task<Result> CreateBackupAsync(string instanceName);

    /// <summary>
    /// Gets the Discord channel ID associated with an instance, if any
    /// </summary>
    /// <param name="instanceName">Name of the instance</param>
    /// <returns>Channel ID, if configured</returns>
    Task<Result<ulong?>> GetChannelIdAsync(string instanceName);
}
