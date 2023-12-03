namespace TheKrystalShip.Admiral.Domain
{
    public enum ExecutionsStatus
    {
        Success,
        Error
    }

    public enum ServiceStatus
    {
        Active,
        Killed,
        Error
    }

    public enum GameServerStatus
    {
        Online,
        Offline,
        Restarting,
        NeedsUpdate,
        Error
    }
}
