namespace KGSM.Bot.Infrastructure.Configuration;

/// <summary>
/// Configuration options for KGSM
/// </summary>
public class KgsmOptions
{
    public const string Section = "KGSM";

    public string Path { get; set; } = string.Empty;
    public string SocketPath { get; set; } = string.Empty;
    public Dictionary<string, BlueprintSettings> Blueprints { get; set; } = new();
    public Dictionary<string, InstanceSettings> Instances { get; set; } = new();
}

/// <summary>
/// Configuration options for blueprints
/// </summary>
public class BlueprintSettings
{
    public string OnlineTrigger { get; set; } = string.Empty;
}

/// <summary>
/// Configuration options for instances
/// </summary>
public class InstanceSettings
{
    public string ChannelId { get; set; } = string.Empty;
    public string Blueprint { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"ChannelId: {ChannelId}, Blueprint: {Blueprint}";
    }
}
