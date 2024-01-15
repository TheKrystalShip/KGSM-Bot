using TheKrystalShip.Admiral.Domain;
using TheKrystalShip.Admiral.Tools;

namespace TheKrystalShip.Admiral.Services
{
    public class CommandExecutioner : ICommandExecutioner
    {
        protected string START_SCRIPT = AppSettings.Get("scripts:start");
        protected string STOP_SCRIPT = AppSettings.Get("scripts:stop");
        protected string RESTART_SCRIPT = AppSettings.Get("scripts:restart");
        protected string STATUS_SCRIPT = AppSettings.Get("scripts:status");

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
            => _internalExecutioner.Execute(command, args);

        public CommandExecutionResult Start(string process)
            => _internalExecutioner.Execute(START_SCRIPT, [process]);

        public CommandExecutionResult Stop(string process)
            => _internalExecutioner.Execute(STOP_SCRIPT, [process]);

        public CommandExecutionResult Restart(string process)
            => _internalExecutioner.Execute(RESTART_SCRIPT, [process]);

        public CommandExecutionResult Status(string process)
            => _internalExecutioner.Execute(STATUS_SCRIPT, [process]);
    }
}
