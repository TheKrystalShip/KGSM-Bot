using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheKrystalShip.KGSM.Domain;

namespace TheKrystalShip.KGSM.Services;

public class KGSMInterop : IInterop
{
    private readonly IInterop _interop;
    private readonly IConfiguration _configuration;
    private readonly string _kgsmScript;

    public KGSMInterop(IServiceProvider services, IConfiguration configuration)
    {
        _interop = services.GetRequiredService<ProcessInterop>();
        _configuration = configuration;

        string? kgsmScript = _configuration["kgsmRoot"] ??
            throw new ArgumentException("kgsmRoot config var not found");

        _kgsmScript = $"{kgsmScript}/kgsm.sh";
    }

    public Result Execute(string command, string[] args)
        => _interop.Execute(command, args);

    public Result Start(string instance)
        => _interop.Execute(_kgsmScript, ["--instance", instance, "--start"]);

    public Result Stop(string instance)
        => _interop.Execute(_kgsmScript, ["--instance", instance, "--stop"]);

    public Result Restart(string instance)
        => _interop.Execute(_kgsmScript, ["--instance", instance, "--restart"]);

    public Result Status(string instance)
        => _interop.Execute(_kgsmScript, ["--instance", instance, "--status"]);

    public Result Info(string instance)
        => _interop.Execute(_kgsmScript, ["--instance", instance, "--info"]);

    public Result IsActive(string instance)
        => _interop.Execute(_kgsmScript, ["--instance", instance, "--is-active"]);

    public Result GetLogs(string instance)
        => _interop.Execute(_kgsmScript, ["--instance", instance, "--logs"]);

    public Result GetIp()
        => _interop.Execute(_kgsmScript, ["--ip"]);
}
