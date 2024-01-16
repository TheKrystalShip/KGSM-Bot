namespace TheKrystalShip.Admiral.Domain
{
    public class RunningStatusUpdatedArgs(string game, RunningStatus runningStatus) : EventArgs
    {
        public string Game { get; set; } = game;
        public RunningStatus RunningStatus { get; set; } = runningStatus;
    }
}
