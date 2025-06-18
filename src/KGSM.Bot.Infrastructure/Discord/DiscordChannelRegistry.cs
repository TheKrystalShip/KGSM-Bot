using KGSM.Bot.Core.Common;
using KGSM.Bot.Core.Interfaces;
using KGSM.Bot.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Discord;
using Discord.WebSocket;

namespace KGSM.Bot.Infrastructure.Discord;

/// <summary>
/// Implementation of IDiscordChannelRegistry
/// </summary>
public class DiscordChannelRegistry : IDiscordChannelRegistry
{
    private readonly DiscordSocketClient _discordClient;
    private readonly DiscordOptions _discordOptions;
    private readonly KgsmOptions _kgsmOptions;
    private readonly ILogger<DiscordChannelRegistry> _logger;
    private readonly Dictionary<string, ulong> _instanceChannels = new();

    public DiscordChannelRegistry(
        DiscordSocketClient discordClient,
        IOptions<DiscordOptions> discordOptions,
        IOptions<KgsmOptions> kgsmOptions,
        ILogger<DiscordChannelRegistry> logger)
    {
        _discordClient = discordClient;
        _discordOptions = discordOptions.Value;
        _kgsmOptions = kgsmOptions.Value;
        _logger = logger;

        // Initialize instance channels from configuration
        foreach (var (instanceName, settings) in _kgsmOptions.Instances)
        {
            if (ulong.TryParse(settings.ChannelId, out var channelId))
            {
                _instanceChannels[instanceName] = channelId;
            }
        }
    }

    /// <inheritdoc />
    public async Task<Result> AddOrUpdateChannelAsync(ulong guildId, string blueprintName, string instanceName)
    {
        try
        {
            _logger.LogInformation(
                "Adding or updating channel for instance {InstanceName} using blueprint {BlueprintName}",
                instanceName, blueprintName);

            // Get the guild
            var guild = _discordClient.GetGuild(guildId);
            if (guild == null)
            {
                _logger.LogWarning("Could not find guild with ID {GuildId}", guildId);
                return Result.Failure($"Could not find guild with ID {guildId}");
            }

            // Get the instances category
            var category = guild.GetCategoryChannel(_discordOptions.InstancesCategoryId);
            if (category == null)
            {
                _logger.LogWarning("Could not find instances category with ID {CategoryId}",
                    _discordOptions.InstancesCategoryId);
                return Result.Failure("Could not find instances category");
            }

            // Check if a channel already exists for this instance
            ITextChannel? channel = null;

            if (_instanceChannels.TryGetValue(instanceName, out var existingChannelId))
            {
                channel = guild.GetTextChannel(existingChannelId);
            }

            // Create a new channel if one doesn't exist
            if (channel == null)
            {
                // Create a new channel
                channel = await guild.CreateTextChannelAsync(instanceName, properties =>
                {
                    properties.CategoryId = category.Id;
                });

                _logger.LogInformation("Created new channel {ChannelName} for instance {InstanceName}",
                    channel.Name, instanceName);
            }

            // Update the channel registry
            _instanceChannels[instanceName] = channel.Id;

            // Update the configuration
            if (!_kgsmOptions.Instances.TryGetValue(instanceName, out var instanceSettings))
            {
                _kgsmOptions.Instances[instanceName] = new InstanceSettings
                {
                    Blueprint = blueprintName,
                    ChannelId = channel.Id.ToString()
                };
            }
            else
            {
                instanceSettings.Blueprint = blueprintName;
                instanceSettings.ChannelId = channel.Id.ToString();
            }

            _logger.LogInformation(
                "Successfully added or updated channel {ChannelName} for instance {InstanceName}",
                channel.Name, instanceName);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding or updating channel for instance {InstanceName}", instanceName);
            return Result.Failure(ex.Message);
        }
    }

    /// <inheritdoc />
    public async Task<Result> RemoveChannelAsync(ulong guildId, string instanceName)
    {
        try
        {
            _logger.LogInformation("Removing channel for instance {InstanceName}", instanceName);

            // Check if we should remove the channel
            if (!_discordOptions.RemoveChannelOnInstanceDeletion)
            {
                _logger.LogInformation("Channel removal is disabled, skipping");

                // Still remove from registry and configuration
                _instanceChannels.Remove(instanceName);
                _kgsmOptions.Instances.Remove(instanceName);

                return Result.Success();
            }

            // Check if a channel exists for this instance
            if (!_instanceChannels.TryGetValue(instanceName, out var channelId))
            {
                _logger.LogWarning("No channel found for instance {InstanceName}", instanceName);
                return Result.Success(); // Not an error, just nothing to do
            }

            // Get the guild
            var guild = _discordClient.GetGuild(guildId);
            if (guild == null)
            {
                _logger.LogWarning("Could not find guild with ID {GuildId}", guildId);
                return Result.Failure($"Could not find guild with ID {guildId}");
            }

            // Get the channel
            var channel = guild.GetTextChannel(channelId);
            if (channel != null)
            {
                // Delete the channel
                await channel.DeleteAsync();
                _logger.LogInformation("Deleted channel {ChannelName} for instance {InstanceName}",
                    channel.Name, instanceName);
            }
            else
            {
                _logger.LogWarning("Channel with ID {ChannelId} not found", channelId);
            }

            // Update the registry and configuration
            _instanceChannels.Remove(instanceName);
            _kgsmOptions.Instances.Remove(instanceName);

            _logger.LogInformation("Successfully removed channel for instance {InstanceName}", instanceName);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing channel for instance {InstanceName}", instanceName);
            return Result.Failure(ex.Message);
        }
    }

    /// <inheritdoc />
    public Task<Result<ulong>> GetChannelIdAsync(string instanceName)
    {
        try
        {
            _logger.LogDebug("Getting channel ID for instance {InstanceName}", instanceName);

            if (!_instanceChannels.TryGetValue(instanceName, out var channelId))
            {
                _logger.LogWarning("No channel found for instance {InstanceName}", instanceName);
                return Task.FromResult(Result.Failure<ulong>($"No channel found for instance {instanceName}"));
            }

            _logger.LogDebug("Found channel ID {ChannelId} for instance {InstanceName}",
                channelId, instanceName);
            return Task.FromResult(Result.Success(channelId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting channel ID for instance {InstanceName}", instanceName);
            return Task.FromResult(Result.Failure<ulong>(ex.Message));
        }
    }
}
