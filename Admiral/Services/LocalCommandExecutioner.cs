using System.Diagnostics;
using TheKrystalShip.Admiral.Domain;
using TheKrystalShip.Logging;

namespace TheKrystalShip.Admiral.Services
{
    /// <summary>
    /// Used to run commands locally on the same machine as the game servers
    /// </summary>
    public class LocalCommandExecutioner : ICommandExecutioner
    {
        private readonly Logger<LocalCommandExecutioner> _logger;

        public LocalCommandExecutioner()
        {
            _logger = new();
        }

        public CommandExecutionResult Execute(string command, string[] args)
        {
            ProcessStartInfo runningInfo = new ProcessStartInfo()
            {
                FileName = command,
                Arguments = string.Join(" ", args),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            Process? process = Process.Start(runningInfo);

            if (process is null)
            {
                _logger.LogError("Process failed to start");
                return new CommandExecutionResult(ExecutionsStatus.Error, "Process failed to start");
            }

            process.WaitForExit();

            int exitCode = process.ExitCode;
            string output = process.StandardOutput.ReadToEnd();

            if (exitCode is 0)
            {
                return new CommandExecutionResult(output);
            }

            return new CommandExecutionResult(ExecutionsStatus.Error, output);
        }
    }
}
