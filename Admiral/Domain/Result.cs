using TheKrystalShip.Admiral.Tools;

namespace TheKrystalShip.Admiral.Domain
{
    public class Result
    {
        public CommandStatus Status { get; set; }
        public string Output { get; set; }

        public Result() : this(CommandStatus.Success, string.Empty) {}
        public Result(CommandStatus status) : this(status, string.Empty) {}
        public Result(string output) : this(CommandStatus.Success, output) {}

        public Result(CommandStatus status, string output)
        {
            Status = status;
            Output = output;
        }

        public override string ToString()
        {
            return $"{Status} - {Output.FirstCharToUpper()}";
        }
    }
}
