using TheKrystalShip.Admiral.Tools;
using Renci.SshNet;
using TheKrystalShip.Admiral.Domain;

namespace TheKrystalShip.Admiral.Services
{
    /// <summary>
    /// Used to interact with the server via ssh.
    /// Automatically connects using login details from appsettings.json
    /// </summary>
    public class SshCommandExecutioner : ICommandExecutioner
    {
        private readonly SshClient _sshClient;
        private readonly string _sudoStringPrepend = $"echo -e '{AppSettings.Get("ssh:password")}' | sudo -S ";

        public SshCommandExecutioner()
        {
            string? sshHost = AppSettings.Get("ssh:host");
            string? sshPort = AppSettings.Get("ssh:port");
            string? sshUsername = AppSettings.Get("ssh:username");
            string? sshPassword = AppSettings.Get("ssh:password");

            if (sshHost is null || sshPort is null || sshUsername is null || sshPassword is null)
            {
                throw new ArgumentNullException("One or more connection details were null");
            }

            try
            {
                int port = int.Parse(sshPort);
                _sshClient = new SshClient(sshHost, port, sshUsername, sshPassword);
                _sshClient.Connect();
                Console.WriteLine("SSH Connection established to {0}", sshHost);
            }
            catch (ArgumentException argEx)
            {
                Console.WriteLine(argEx.Message);
                throw;
            }
        }

        public CommandExecutionResult Execute(string command, string[] args)
        {
            string argsCommand = string.Concat(command + " ", string.Join(" ", args));

            SshCommand result = _sshClient.RunCommand(argsCommand);
            string executionResult;

            // Success
            if (result.ExitStatus is 0)
            {
                executionResult = result.Execute();
                return new CommandExecutionResult(executionResult);
            }

            // systemctl status returns exit code 3 for some reason
            if (result.ExitStatus == 3 && result.Error == string.Empty)
            {
                executionResult = result.Execute();
                return new CommandExecutionResult(executionResult);
            }

            // TODO: Check for actual error message, don't assume it's lack of sudo
            if (result.Error is not null && result.ExitStatus is not 0)
            {
                return ExecuteWithSudo(command, args);
            }

            executionResult = result.Execute();

            if (executionResult == "")
            {
                return new CommandExecutionResult();
            }

            return new CommandExecutionResult(ExecutionsStatus.Error, executionResult);
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

        private CommandExecutionResult ExecuteWithSudo(string command, string[] args)
        {
            return Execute(_sudoStringPrepend + command, args);
        }
    }
}
