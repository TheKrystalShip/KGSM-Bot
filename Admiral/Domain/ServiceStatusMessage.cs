namespace TheKrystalShip.Admiral.Domain
{
    public record ServiceStatusMessage(string service, int status)
    {
        private const int ACTIVE_STATUS = 1;
        private const int INACTIVE_STATUS = 0;

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
