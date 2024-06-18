using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheKrystalShip.KGSM.Domain;

namespace TheKrystalShip.KGSM.Services;

public class CommandExecutioner : ICommandExecutioner
{
    private readonly ICommandExecutioner _internal;
    private readonly IConfiguration _configuration;

    public CommandExecutioner(IServiceProvider services, IConfiguration configuration)
    {
#if DEBUG
        _internal = services.GetRequiredService<SshCommandExecutioner>();
#else
        _internal = services.GetRequiredService<ProcessCommandExecutioner>();
#endif
        _configuration = configuration;
    }

    public Result Execute(string command, string[] args)
        => _internal.Execute(command, args);

    public Result Start(string process)
        => _internal.Execute(_configuration["scripts:start"] ?? string.Empty, [process]);

    public Result Stop(string process)
        => _internal.Execute(_configuration["scripts:stop"] ?? string.Empty, [process]);

    public Result Restart(string process)
        => _internal.Execute(_configuration["scripts:restart"] ?? string.Empty, [process]);

    public Result Status(string process)
        => _internal.Execute(_configuration["scripts:status"] ?? string.Empty, [process]);

    public Result IsActive(string process)
        => _internal.Execute(_configuration["scripts:is-active"] ?? string.Empty, [process]);

    public Result GetLogs(string process)
        => _internal.Execute(_configuration["scripts:get-logs"] ?? string.Empty, [process]);

    public Result GetIp()
        => _internal.Execute(_configuration["scripts:get-ip"] ?? string.Empty);
}
