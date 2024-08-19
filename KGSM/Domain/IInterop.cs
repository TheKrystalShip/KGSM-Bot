namespace TheKrystalShip.KGSM.Domain
{
    public interface IInterop
    {
        public Result Execute(string command, string[] args);
        public Result Execute(string command) => new();
        public Result Start(string instance) => new();
        public Result Stop(string instance) => new();
        public Result Restart(string instance) => new();
        public Result Status(string instance) => new();
        public Result Info(string instance) => new();
        public Result IsActive(string instance) => new();
        public Result Enable(string instance) => new();
        public Result Disable(string instance) => new();
        public Result CheckForUpdate(string instance) => new();
        public Result GetLogs(string instance) => new();
        public Result GetIp() => new();
    }
}
