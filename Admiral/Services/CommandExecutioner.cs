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

        private readonly ICommandExecutioner _internal;

        public CommandExecutioner()
        {
#if DEBUG
            _internal = new SshCommandExecutioner();
#else
            _internal = new ProcessCommandExecutioner();
#endif
        }

        public Result Execute(string command, string[] args)
            => _internal.Execute(command, args);

        public Result Start(string process)
            => _internal.Execute(START_SCRIPT, [process]);

        public Result Stop(string process)
            => _internal.Execute(STOP_SCRIPT, [process]);

        public Result Restart(string process)
            => _internal.Execute(RESTART_SCRIPT, [process]);

        public Result Status(string process)
            => _internal.Execute(STATUS_SCRIPT, [process]);
    }
}
