using TheKrystalShip.Admiral.Tools;

namespace TheKrystalShip.Admiral.Domain
{
    public class Result
    {
        public CommandStatus Status { get; set; }
        public string Output { get; set; }

        public bool IsSuccess { get => Status == CommandStatus.Success; }
        public bool IsError { get => Status == CommandStatus.Error; }

        public bool IsSuccessWithOutput { get => Status == CommandStatus.Success && Output != string.Empty; }
        public bool IsErrorWithOutput { get => Status == CommandStatus.Error && Output != string.Empty; }

        public bool IsSuccessWithNoOutput { get => Status == CommandStatus.Success && Output == string.Empty; }
        public bool IsErrorWithNoOutput { get => Status == CommandStatus.Error && Output == string.Empty; }

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
            return $"{Status} - {Output}";
        }
    }
}
