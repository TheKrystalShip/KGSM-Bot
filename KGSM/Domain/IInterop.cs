namespace TheKrystalShip.KGSM.Domain
{
    public interface IInterop
    {
        public Result Execute(string command, string[] args);
        public Result Execute(string command) => new();
        public Result Start(string process) => new();
        public Result Stop(string process) => new();
        public Result Restart(string process) => new();
        public Result Status(string process) => new();
        public Result IsActive(string process) => new();
        public Result Enable(string process) => new();
        public Result Disable(string process) => new();
        public Result CheckForUpdate(string process) => new();
        public Result GetLogs(string process) => new();
        public Result GetIp() => new();
    }
}
