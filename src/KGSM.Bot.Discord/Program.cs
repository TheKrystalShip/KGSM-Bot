using KGSM.Bot.Infrastructure;
using KGSM.Bot.Application;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KGSM.Bot.Discord;

/// <summary>
/// Main program
/// </summary>
public class Program
{
    /// <summary>
    /// Application entry point
    /// </summary>
    public static async Task Main(string[] args)
    {
        // Create and configure the host
        using var host = CreateHostBuilder(args).Build();

        // Start the host
        await host.RunAsync();
    }

    /// <summary>
    /// Creates and configures the host builder
    /// </summary>
    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json",
                    optional: true,
                    reloadOnChange: true);
                config.AddEnvironmentVariables();

                if (args != null)
                {
                    config.AddCommandLine(args);
                }
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.AddConfiguration(context.Configuration.GetSection("Logging"));
                logging.AddConsole();
                logging.AddDebug();
            })
            .ConfigureServices((context, services) =>
            {
                // Register application services
                services.AddApplicationServices();

                // Register infrastructure services
                services.AddInfrastructureServices(context.Configuration);

                // Register the interaction handler
                services.AddSingleton<InteractionHandler>();

                // Register hosted service
                services.AddHostedService<BotService>();
            });
}
