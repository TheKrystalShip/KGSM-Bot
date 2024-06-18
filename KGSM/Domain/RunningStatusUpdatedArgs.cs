namespace TheKrystalShip.KGSM.Domain
{
    public class RunningStatusUpdatedArgs(Game game, RunningStatus runningStatus) : EventArgs
    {
        public Game Game { get; set; } = game;
        public RunningStatus RunningStatus { get; set; } = runningStatus;
    }
}
