using System.Diagnostics;
using TheKrystalShip.Admiral.Domain;
using TheKrystalShip.Logging;

namespace TheKrystalShip.Admiral.Services
{
    /// <summary>
    /// Used to run commands locally on the same machine as the game servers
    /// </summary>
    public class ProcessCommandExecutioner : ICommandExecutioner
    {
        private readonly Logger<ProcessCommandExecutioner> _logger;

        public ProcessCommandExecutioner()
        {
            _logger = new();
        }

        public Result Execute(string command, string[] args)
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
                return new Result(CommandStatus.Error, "Process failed to start");
            }

            process.WaitForExit();

            int exitCode = process.ExitCode;
            string stdout = process.StandardOutput.ReadToEnd();
            string stderr = process.StandardError.ReadToEnd();

            if (exitCode == 0 && stderr == string.Empty)
            {
                return new Result(stdout);
            }

            return new Result(CommandStatus.Error, stderr);
        }
    }
}
