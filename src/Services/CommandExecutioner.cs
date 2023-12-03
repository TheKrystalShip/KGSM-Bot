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

        public CommandExecutionResult Execute(string command)
        {
            return _internalExecutioner.Execute(command);
        }
    }
}
