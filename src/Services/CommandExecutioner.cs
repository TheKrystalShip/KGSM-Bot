using TheKrystalShip.Admiral.Domain;

namespace TheKrystalShip.Admiral.Services
{
    public class CommandExecutioner : ICommandExecutioner
    {
        private readonly ICommandExecutioner _internalExecutioner;

        public CommandExecutioner()
        {
#if DEBUG
            _internalExecutioner = new SshCommandExecutioner();
#else
            _internalExecutioner = new LocalCommandExecutioner();
#endif
        }

        public CommandExecutionResult Execute(string command, string[] args)
        {
            return _internalExecutioner.Execute(command, args);
        }

        public CommandExecutionResult Start(string process)
        {
            return _internalExecutioner.Start(process);
        }

        public CommandExecutionResult Stop(string process)
        {
            return _internalExecutioner.Stop(process);
        }

        public CommandExecutionResult Restart(string process)
        {
            return _internalExecutioner.Restart(process);
        }

        public CommandExecutionResult Status(string process)
        {
            return _internalExecutioner.Status(process);
        }
    }
}
