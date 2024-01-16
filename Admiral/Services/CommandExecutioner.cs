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
        protected string IS_ACTIVE_SCRIPT = AppSettings.Get("scripts:is-active");
        protected string IS_ENABLED_SCRIPT = AppSettings.Get("scripts:is-enabled");
        protected string ENABLE_SCRIPT = AppSettings.Get("scripts:enable");
        protected string DISABLE_SCRIPT = AppSettings.Get("scripts:disable");

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

        public Result IsActive(string process)
            => _internal.Execute(IS_ACTIVE_SCRIPT, [process]);

        public Result IsEnabled(string process)
            => _internal.Execute(IS_ENABLED_SCRIPT, [process]);

        public Result Enable(string process)
            => _internal.Execute(ENABLE_SCRIPT, [process]);

        public Result Disable(string process)
            => _internal.Execute(DISABLE_SCRIPT, [process]);

        public Result CheckForUpdate(string process)
        {
            string workingDir = AppSettings.Get($"games:{process}:workingDir");

            if (workingDir == string.Empty)
                return new Result(CommandStatus.Error, $"Failed to locate {process} workingDir");

            string versionCheckScript = AppSettings.Get($"games:{process}:versionCheckScript");

            if (versionCheckScript == string.Empty)
                return new Result(CommandStatus.Error, "Failed to locate versionCheck script");

            string scriptAbsolutePath = $"{workingDir}/{versionCheckScript}";

            return _internal.Execute(scriptAbsolutePath);
        }
    }
}
