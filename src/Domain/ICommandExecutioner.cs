namespace TheKrystalShip.Admiral.Domain
{
    public interface ICommandExecutioner
    {
        public CommandExecutionResult Execute(string command, string[] args);
        public CommandExecutionResult Start(string process);
        public CommandExecutionResult Stop(string process);
        public CommandExecutionResult Restart(string process);
        public CommandExecutionResult Status(string process);
    }
}
