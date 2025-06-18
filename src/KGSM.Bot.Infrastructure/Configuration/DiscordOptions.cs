namespace KGSM.Bot.Infrastructure.Configuration;

/// <summary>
/// Configuration options for Discord
/// </summary>
public class DiscordOptions
{
    public const string Section = "Discord";

    public string Token { get; set; } = string.Empty;
    public ulong GuildId { get; set; }
    public ulong InstancesCategoryId { get; set; }
    public bool RemoveChannelOnInstanceDeletion { get; set; } = false;
    public StatusOptions Status { get; set; } = new();
    public bool DeleteStatusMessageAfterDelay { get; set; } = false;
    public int DeleteStatusMessageDelaySeconds { get; set; } = 10;
}

/// <summary>
/// Configuration options for status messages
/// </summary>
public class StatusOptions
{
    public string Offline { get; set; } = string.Empty;
    public string Online { get; set; } = string.Empty;
    public string Uninstalled { get; set; } = string.Empty;
}
