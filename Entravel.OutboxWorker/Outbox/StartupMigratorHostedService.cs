using Entravel.EF.Infrastructure.Persistence;

namespace Entravel.OutboxWorker.Outbox;

public sealed class StartupMigratorHostedService(IServiceProvider services, IHostEnvironment environment, ILogger<StartupMigratorHostedService> logger)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await DatabaseInitializer.ApplyMigrationsAndSeedAsync(services, environment.EnvironmentName, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to apply migrations on worker startup");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

