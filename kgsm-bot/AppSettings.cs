namespace TheKrystalShip.KGSM;

public class AppSettings
{
    public string KgsmPath { get; set; } = "";
    public string KgsmSocketPath { get; set; } = "";
    public DiscordSettings Discord { get; set; } = new();
    public Dictionary<string, BlueprintSettings> Blueprints { get; set; } = new();
    public Dictionary<string, InstanceSettings> Instances { get; set; } = new();
}

public class DiscordSettings
{
    public string Token { get; set; } = "";
    public string GuildId { get; set; } = "";
    public string InstancesCategoryId { get; set; } = "";
    public StatusSettings Status { get; set; } = new();
}

public class StatusSettings
{
    public string Uninstalled { get; set; } = "";
    public string Offline { get; set; } = "";
    public string Online { get; set; } = "";
    public string NeedsUpdate { get; set; } = "";
    public string Error { get; set; } = "";
}

public class BlueprintSettings
{
    public string OnlineTrigger { get; set; } = "";
    public string OfflineTrigger { get; set; } = "";
}

public class InstanceSettings
{
    public string ChannelId { get; set; } = "";
    public string Blueprint { get; set; } = "";

    public override string ToString()
    {
        return $"ChannelId: {ChannelId}, Blueprint: {Blueprint}";
    }
}

