namespace TheKrystalShip.Admiral.Domain
{
    public interface ICommandExecutioner
    {
        public CommandExecutionResult Execute(string command, string[] args);
        public CommandExecutionResult Start(string process) => new();
        public CommandExecutionResult Stop(string process) => new();
        public CommandExecutionResult Restart(string process) => new();
        public CommandExecutionResult Status(string process) => new();
    }
}
