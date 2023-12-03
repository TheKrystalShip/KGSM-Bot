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
            string? sshUsername = AppSettings.Get("ssh:username");
            string? sshPassword = AppSettings.Get("ssh:password");

            if (sshHost is null || sshUsername is null || sshPassword is null)
            {
                throw new ArgumentNullException("One or more connection details were null");
            }

            try
            {
                _sshClient = new SshClient(sshHost, sshUsername, sshPassword);
                _sshClient.Connect();
                Console.WriteLine("SSH Connection established to {0}", sshHost);
            }
            catch (ArgumentException argEx)
            {
                Console.WriteLine(argEx.Message);
                throw;
            }
        }

        public CommandExecutionResult Execute(string command)
        {
            SshCommand result = _sshClient.RunCommand(command);
            string executionResult;

            // systemctl status returns exit code 3 for some reason
            if (result.ExitStatus == 3 && result.Error == string.Empty)
            {
                executionResult = result.Execute();
                return new CommandExecutionResult(ExecutionsStatus.Success, executionResult);
            }

            // TODO: Check for actual error message, don't assume it's lack of sudo
            if (result.Error is not null && result.ExitStatus is not 0)
            {
                return ExecuteWithSudo(command);
            }

            executionResult = result.Execute();

            if (executionResult == "")
            {
                return new CommandExecutionResult();
            }

            return new CommandExecutionResult(ExecutionsStatus.Error, executionResult);
        }

        private CommandExecutionResult ExecuteWithSudo(string command)
        {
            return Execute(_sudoStringPrepend + command);
        }
    }
}
