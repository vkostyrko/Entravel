using System.Reflection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Entravel.EF.DbContext;
using Entravel.Data.Repositories;
using Entravel.Data.Outbox;
using Entravel.EF.MappingProfile;
using Entravel.EF.Outbox;
using Entravel.Data.Queries;
using Entravel.EF.Queries;
using Entravel.EF.Repositories;

namespace Entravel.EF.Dependencies;

public static class EntityFrameworkDependencyInjection
{
    public static IServiceCollection AddEfInfrastructure(this IServiceCollection services)
    {
        var configuration = BuildEfConfiguration();
        return services.AddEfInfrastructure(configuration);
    }

    public static IServiceCollection AddEfInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Database connection string not configured");
        }

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderReadRepository, OrderReadRepository>();
        services.AddScoped<IOrderProcessingRepository, OrderProcessingRepository>();
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
        services.AddScoped<IOutboxProcessingRepository, OutboxProcessingRepository>();

        AddPersistenceAutoMapper(services);

        return services;
    }

    private static void AddPersistenceAutoMapper(IServiceCollection services)
    {
        var persistenceAssembly = typeof(OrderEntityMappingProfile).Assembly;
        var entryAssembly = Assembly.GetEntryAssembly();

        if (entryAssembly is not null
            && !string.Equals(entryAssembly.GetName().Name, persistenceAssembly.GetName().Name, StringComparison.Ordinal))
        {
            services.AddAutoMapper(entryAssembly, persistenceAssembly);
            return;
        }

        services.AddAutoMapper(persistenceAssembly);
    }

    private static IConfiguration BuildEfConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("entravel.efsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
    }
}

