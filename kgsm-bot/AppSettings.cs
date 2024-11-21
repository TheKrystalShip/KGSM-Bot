namespace TheKrystalShip.KGSM;

public class AppSettings
{
    public DiscordSettings Discord { get; set; } = new();
    public KgsmSettings Kgsm { get; set; } = new();
}

public class DiscordSettings
{
    public string Token { get; set; } = "";
    public ulong Guild { get; set; } = 0;
    public ulong InstancesCategoryId { get; set; } = 0;
    public StatusSettings Status { get; set; } = new();
}

public class StatusSettings
{
    public string Offline { get; set; } = "";
    public string Online { get; set; } = "";
}

public class KgsmSettings
{
    public string Path { get; set; } = "";
    public string SocketPath { get; set; } = "";
    public Dictionary<string, BlueprintSettings> Blueprints { get; set; } = new();
    public Dictionary<string, InstanceSettings> Instances { get; set; } = new();
}

public class BlueprintSettings
{
    public string OnlineTrigger { get; set; } = "";
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

