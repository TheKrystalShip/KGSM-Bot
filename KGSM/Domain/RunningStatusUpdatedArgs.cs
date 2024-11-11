namespace TheKrystalShip.KGSM.Domain
{
    public class RunningStatusUpdatedArgs(string instanceId, RunningStatus runningStatus) : EventArgs
    {
        public string InstanceId { get; set; } = instanceId;
        public RunningStatus RunningStatus { get; set; } = runningStatus;
    }
}
