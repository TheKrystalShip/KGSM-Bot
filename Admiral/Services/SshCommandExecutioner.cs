using TheKrystalShip.Admiral.Tools;
using Renci.SshNet;
using TheKrystalShip.Admiral.Domain;
using TheKrystalShip.Logging;

namespace TheKrystalShip.Admiral.Services
{
    /// <summary>
    /// Used to interact with the server via ssh.
    /// Automatically connects using login details from appsettings.json
    /// </summary>
    public class SshCommandExecutioner : ICommandExecutioner
    {
        private readonly Logger<SshCommandExecutioner> _logger;
        private readonly SshClient _sshClient;
        private readonly string _sudoStringPrepend = $"echo -e '{AppSettings.Get("ssh:password")}' | sudo -S ";

        public SshCommandExecutioner()
        {
            _logger = new();

            string sshHost = AppSettings.Get("ssh:host");
            string sshPort = AppSettings.Get("ssh:port");
            string sshUsername = AppSettings.Get("ssh:username");
            string sshPassword = AppSettings.Get("ssh:password");

            if (sshHost == string.Empty || sshPort == string.Empty || sshUsername == string.Empty || sshPassword == string.Empty)
            {
                throw new ArgumentNullException("One or more connection details were null");
            }

            int port = int.Parse(sshPort);
            _sshClient = new SshClient(sshHost, port, sshUsername, sshPassword);

            try
            {
                _sshClient.Connect();
                _logger.LogInformation($"SSH Connection established to {sshHost}");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
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

        private CommandExecutionResult ExecuteWithSudo(string command, string[] args)
        {
            return Execute(_sudoStringPrepend + command, args);
        }
    }
}
