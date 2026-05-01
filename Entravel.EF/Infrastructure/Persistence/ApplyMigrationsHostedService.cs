using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Entravel.EF.Infrastructure.Persistence;

public sealed class ApplyMigrationsHostedService(
    IServiceProvider services,
    IHostEnvironment environment,
    ILogger<ApplyMigrationsHostedService> logger)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await DatabaseInitializer.ApplyMigrationsAsync(services, environment.EnvironmentName, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to apply database migrations on startup");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
