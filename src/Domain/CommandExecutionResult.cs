namespace TheKrystalShip.Admiral.Domain
{
    public class CommandExecutionResult
    {
        public ExecutionsStatus Status { get; set; }
        public string Output { get; set; }

        public CommandExecutionResult()
        {
            Status = ExecutionsStatus.Success;
            Output = "";
        }

        public CommandExecutionResult(ExecutionsStatus status, string output)
        {
            Status = status;
            Output = output;
        }
    }
}
