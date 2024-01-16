namespace TheKrystalShip.Admiral.Domain
{
    public enum CommandStatus
    {
        Success,
        Error,
        Ignore
    }

    public enum ServiceStatus
    {
        Active,
        Killed,
        Error
    }

    public enum RunningStatus
    {
        Online,
        Offline,
        Restarting,
        NeedsUpdate,
        Error
    }
}
