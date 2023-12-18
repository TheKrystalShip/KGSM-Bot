using System.Diagnostics;
using TheKrystalShip.Admiral.Domain;
using TheKrystalShip.Admiral.Tools;

namespace TheKrystalShip.Admiral.Services
{
    /// <summary>
    /// Used to run commands locally on the same machine as the game servers
    /// </summary>
    public class LocalCommandExecutioner : ICommandExecutioner
    {
        public LocalCommandExecutioner()
        {

        }

        public CommandExecutionResult Execute(string command, string[] args)
        {
            ProcessStartInfo runningInfo = new ProcessStartInfo()
            {
                FileName = command,
                Arguments = string.Join(" ", args),
                UseShellExecute = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            Process? process = Process.Start(runningInfo);

            if (process is null)
            {
                Console.WriteLine("Process failed to start");
                return new CommandExecutionResult(ExecutionsStatus.Error, "Process failed to start");
            }

            int exitCode = process.ExitCode;
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (exitCode is 0)
            {
                return new CommandExecutionResult(output);
            }

            return new CommandExecutionResult(ExecutionsStatus.Error, output);
        }

        public CommandExecutionResult Start(string process)
        {
            string? startScript = AppSettings.Get("scripts:start");

            if (startScript is null)
            {
                throw new ArgumentNullException(startScript);
            }

            return Execute(startScript, [process]);
        }

        public CommandExecutionResult Stop(string process)
        {
            string? stopScript = AppSettings.Get("scripts:stop");

            if (stopScript is null)
            {
                throw new ArgumentNullException(stopScript);
            }

            return Execute(stopScript, [process]);
        }

        public CommandExecutionResult Restart(string process)
        {
            string? restartScript = AppSettings.Get("scripts:restart");

            if (restartScript is null)
            {
                throw new ArgumentNullException(restartScript);
            }

            return Execute(restartScript, [process]);
        }

        public CommandExecutionResult Status(string process)
        {
            string? statusScript = AppSettings.Get("scripts:status");

            if (statusScript is null)
            {
                throw new ArgumentNullException(statusScript);
            }

            return Execute(statusScript, [process]);
        }
    }
}
