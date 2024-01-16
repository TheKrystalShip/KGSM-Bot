namespace TheKrystalShip.Admiral.Domain
{
    public interface ICommandExecutioner
    {
        public Result Execute(string command, string[] args);
        public Result Start(string process) => new();
        public Result Stop(string process) => new();
        public Result Restart(string process) => new();
        public Result Status(string process) => new();
        public Result IsActive(string process) => new();
        public Result IsEnabled(string process) => new();
        public Result Enable(string process) => new();
        public Result Disable(string process) => new();
    }
}
