namespace TheKrystalShip.Admiral.Domain
{
    public record ServiceStatusMessage(string service, string status)
    {
        private const string ACTIVE_STATUS = "active";
        private const string INACTIVE_STATUS = "inactive";

        public bool IsOnline => status == ACTIVE_STATUS;
        public bool IsOffline => status == INACTIVE_STATUS;

        public RunningStatus RunningStatus =>
            status switch
            {
                ACTIVE_STATUS => RunningStatus.Online,
                INACTIVE_STATUS => RunningStatus.Offline,
                _ => RunningStatus.Offline
            };

        public override string ToString()
        {
            return $"Service: {service}, Status: {status}";
        }
    }
}
