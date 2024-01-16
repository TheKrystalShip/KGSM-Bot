#if DEBUG
// Don't include this file in prod

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

        public Result Execute(string command, string[] args)
        {
            string argsCommand = string.Concat(command + " ", string.Join(" ", args));

            SshCommand commandExecution = _sshClient.RunCommand(argsCommand);

            /*
            * Properties:
            *     - Result: (can be empty) string, not null
            *     - Error: (can be empty) string, not null
            *     - ExitStatus: int
            * RULES:
            *     - ExitStatus different than 0 doesn't always indicate there's ben an error, don't trust it.
            *     - Result can be empty after a successful command, check Error & ExitStatus
            *     - Error can be empty but an error might have still ocurred, check Result & ExitStatus
            */
            return commandExecution switch
            {
                // Success
                // Result is empty, ExitStatus is 0, Error is empty, treat as success
                { Result: var result, ExitStatus: var exitStatus, Error: var error }
                    when (result == string.Empty) && (exitStatus == 0) && (error == string.Empty) => new Result(),

                // Success
                // Error is empty but Result contains something, treat as success
                { Result: var result, Error: var error }
                    when (result != string.Empty) && (error == string.Empty) => new Result(result),

                // Fail
                // ExitStatus is 0 but Error contains something, treat as fail
                { ExitStatus: var exitStatus, Error: var error }
                    when (exitStatus == 0) && (error != string.Empty) => new Result(CommandStatus.Error, error),

                // No f-ing clue
                // Result is not empty, ExitStatus is not 0 but Error is empty, treat as... fail, just to be sure
                { Result: var result, ExitStatus: var exitStatus, Error: var error }
                    when (result != string.Empty) && (exitStatus != 0) && (error == string.Empty) => new Result(CommandStatus.Error, result),

                // Default to error
                _ => new Result(CommandStatus.Error, commandExecution.Result)
            };
        }
    }
}

#endif
