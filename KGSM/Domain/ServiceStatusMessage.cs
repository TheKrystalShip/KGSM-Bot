namespace TheKrystalShip.KGSM.Domain
{
    public record ServiceStatusMessage(string service, RunningStatus status)
    {
        public override string ToString()
        {
            return $"Service: {service}, Status: {status}";
        }
    }
}
