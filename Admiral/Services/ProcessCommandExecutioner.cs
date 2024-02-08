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

        public Result Execute(string command)
            => Execute(command, []);

        public Result Execute(string command, string[] args)
        {
            ProcessStartInfo runningInfo = new ProcessStartInfo()
            {
                FileName = command,
                Arguments = string.Join(" ", args),
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            Process? process = Process.Start(runningInfo);

            if (process is null)
            {
                _logger.LogError("Process is null");
                return new Result(CommandStatus.Error, "Process failed to start");
            }

            process.WaitForExit();

            int exitCode = process.ExitCode;
            string stdout = process.StandardOutput.ReadToEnd().Trim();
            string stderr = string.Empty;

            // Random intermittent error that StandardError has not been redirected
            // when it clearly has...
            try {
                stderr = process.StandardError.ReadToEnd();
            } catch (Exception e) {
                _logger.LogError(e);
            }

            // Exit code 0 and no error probably means success
            if (exitCode == 0 && stderr == string.Empty)
                return new Result(stdout);

            // Default to error
            return new Result(CommandStatus.Error, stderr);
        }
    }
}
