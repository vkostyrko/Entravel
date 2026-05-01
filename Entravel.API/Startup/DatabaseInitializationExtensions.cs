using Entravel.EF.Infrastructure.Persistence;

namespace Entravel.API.Startup;

public static class DatabaseInitializationExtensions
{
    public static Task ApplyDatabaseMigrationsAndSeedAsync(this WebApplication app, CancellationToken cancellationToken = default) =>
        DatabaseInitializer.InitializeAsync(app.Services, app.Environment.EnvironmentName, cancellationToken);
}

Щгеищч