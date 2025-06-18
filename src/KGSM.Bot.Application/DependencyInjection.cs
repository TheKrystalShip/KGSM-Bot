using KGSM.Bot.Application.Services;

using Microsoft.Extensions.DependencyInjection;

namespace KGSM.Bot.Application;

/// <summary>
/// Extension methods for registering application services
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register MediatR handlers from this assembly
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });

        // Register application services
        services.AddTransient<ServerEventCoordinatorService>();

        return services;
    }
}
