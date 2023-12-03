namespace TheKrystalShip.Admiral.Domain
{
    public interface ICommandExecutioner
    {
        public CommandExecutionResult Execute(string command);
    }
}
