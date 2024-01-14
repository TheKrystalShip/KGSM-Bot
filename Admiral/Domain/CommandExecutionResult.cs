namespace TheKrystalShip.Admiral.Domain
{
    public class CommandExecutionResult
    {
        public ExecutionsStatus Status { get; set; }
        public string Output { get; set; }

        public CommandExecutionResult()
        {
            Status = ExecutionsStatus.Success;
            Output = string.Empty;
        }

        public CommandExecutionResult(string output)
        {
            Status = ExecutionsStatus.Success;
            Output = output;
        }

        public CommandExecutionResult(ExecutionsStatus status, string output)
        {
            Status = status;
            Output = output;
        }
    }
}
