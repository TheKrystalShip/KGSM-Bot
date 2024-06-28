#if DEBUG
// Don't include this file in prod

using Microsoft.Extensions.Configuration;
using Renci.SshNet;
using TheKrystalShip.KGSM.Domain;
using TheKrystalShip.Logging;

namespace TheKrystalShip.KGSM.Services;

/// <summary>
/// Used to interact with the server via ssh.
/// Automatically connects using login details from appsettings.json
/// </summary>
public class SshCommandExecutioner : IInterop
{
    private readonly Logger<SshCommandExecutioner> _logger;
    private readonly IConfiguration _configuration;
    private readonly SshClient _sshClient;

    public SshCommandExecutioner(IConfiguration configuration)
    {
        _configuration = configuration;
        _logger = new();

        string sshHost = _configuration["ssh:host"] ?? throw new ArgumentNullException(nameof(sshHost));
        string sshPort = _configuration["ssh:port"] ?? throw new ArgumentNullException(nameof(sshPort));
        string sshUsername = _configuration["ssh:username"] ?? throw new ArgumentNullException(nameof(sshUsername));
        string sshPassword = _configuration["ssh:password"] ?? throw new ArgumentNullException(nameof(sshPassword));

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

    public Result Execute(string command)
        => Execute(command, []);

    public Result Execute(string command, string[] args)
    {
        string argsCommand = string.Concat(command + " ", string.Join(" ", args));

        SshCommand commandExecution = _sshClient.RunCommand(argsCommand);

        /*
        * _jesus christ..._
        * Luckily all of this only exists for running it locally, prod doesn't see any of this code
        *
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
            // Result has something, ExitStatus is 0
            { Result: var result, ExitStatus: var exitStatus }
                when (result != string.Empty) && (exitStatus == 0) => new Result(result.Trim()),

            // Success
            // Result is empty, ExitStatus is 0, Error is empty, treat as success
            { Result: var result, ExitStatus: var exitStatus, Error: var error }
                when (result == string.Empty) && (exitStatus == 0) && (error == string.Empty) => new Result(),

            // Success?
            // Result is empty, ExitStatus is 0 but Error has something
            // This happens when using the `systemctl enable/disable` command, for some reason the output is in Error
            // instead of being in Result 🤦‍♂️
            { Result: var result, ExitStatus: var exitStatus, Error: var error }
                when (result == string.Empty) && (exitStatus == 0) && (error != string.Empty) => new Result(CommandStatus.Success, error.Trim()),

            // Default to error
            _ => new Result(CommandStatus.Error, commandExecution.Error.Trim())
        };
    }
}

#endif
