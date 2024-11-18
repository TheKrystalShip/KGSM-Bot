using System.Text.Json;

namespace TheKrystalShip.KGSM.Domain;

public abstract class EventDataBase { }

public class EventWrapper
{
    public string EventType { get; set; }
    public JsonElement Data { get; set; } // Using JsonElement for dynamic deserialization
}

public class InstanceInstalledData : EventDataBase
{
    public string InstanceId { get; set; }
    public string Blueprint { get; set; }
}

public class InstanceUninstalledData : EventDataBase
{
    public string InstanceId { get; set; }
}

public class InstanceStartedData : EventDataBase
{
    public string InstanceId { get; set; }
}

public class InstanceStoppedData : EventDataBase
{
    public string InstanceId { get; set; }
}


