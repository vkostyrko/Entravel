using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Entravel.EF.DbContext;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    private const string SettingsFileName = "entravel.efsettings.json";

    public AppDbContext CreateDbContext(string[] args)
    {
        var settingsPath = ResolveSettingsPath();

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(settingsPath, optional: false, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Database connection string not configured");
        }

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }

    private static string ResolveSettingsPath()
    {
        var currentDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), SettingsFileName);

        if (File.Exists(currentDirectoryPath))
        {
            return currentDirectoryPath;
        }

        var solutionRelativePath = Path.Combine(Directory.GetCurrentDirectory(), "Entravel.EF", SettingsFileName);

        if (File.Exists(solutionRelativePath))
        {
            return solutionRelativePath;
        }

        var baseDirectoryPath = Path.Combine(AppContext.BaseDirectory, SettingsFileName);

        if (File.Exists(baseDirectoryPath))
        {
            return baseDirectoryPath;
        }

        throw new InvalidOperationException(
            $"The configuration file '{SettingsFileName}' was not found. Searched: '{currentDirectoryPath}', '{solutionRelativePath}', '{baseDirectoryPath}'.");
    }
}

