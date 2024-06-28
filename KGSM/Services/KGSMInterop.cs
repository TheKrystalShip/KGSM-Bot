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

        string? kgsmScript = _configuration["KGSM_ROOT"] ??
            throw new ArgumentException("KGSM_ROOT environmental variable not found");

        _kgsmScript = $"{kgsmScript}/kgsm.sh";
    }

    public Result Execute(string command, string[] args)
        => _interop.Execute(command, args);

    public Result Start(string process)
        => _interop.Execute(_kgsmScript, ["--service", process, "--start"]);

    public Result Stop(string process)
        => _interop.Execute(_kgsmScript, ["--service", process, "--stop"]);

    public Result Restart(string process)
        => _interop.Execute(_kgsmScript, ["--service", process, "--restart"]);

    public Result Status(string process)
        => _interop.Execute(_kgsmScript, ["--service", process, "--status"]);

    public Result IsActive(string process)
        => _interop.Execute(_kgsmScript, ["--service", process, "--is-active"]);

    public Result GetLogs(string process)
        => _interop.Execute(_kgsmScript, ["--service", process, "--logs"]);

    public Result GetIp()
        => _interop.Execute(_kgsmScript, ["--ip"]);
}
