using Entravel.EF.DbContext;
using Entravel.EF.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Entravel.EF.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    private static readonly string[] AllowedEnvironments = ["Development", "Docker", "Local"];

    public static async Task ApplyMigrationsAndSeedAsync(IServiceProvider services, string environmentName, CancellationToken cancellationToken = default)
    {
        if (!AllowedEnvironments.Contains(environmentName, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync(cancellationToken);
        await SeedAsync(db, cancellationToken);
    }

    private static async Task SeedAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        await SeedCustomersAsync(db, cancellationToken);
        await SeedInventoryAsync(db, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedCustomersAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        if (await db.Customers.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;

        db.Customers.AddRange(
            new CustomerEntity { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Acme Travel", Email = "acme@example.com", CreatedDate = now },
            new CustomerEntity { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Globex Corp", Email = "globex@example.com", CreatedDate = now },
            new CustomerEntity { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Umbrella Ltd", Email = "umbrella@example.com", CreatedDate = now }
        );
    }

    private static async Task SeedInventoryAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        if (await db.Inventory.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;

        db.Inventory.AddRange(
            new InventoryEntity { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Name = "City tour", Price = 49.99m, AvailableQuantity = 100, CreatedDate = now },
            new InventoryEntity { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Name = "Airport transfer", Price = 19.50m, AvailableQuantity = 250, CreatedDate = now },
            new InventoryEntity { Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), Name = "Museum ticket", Price = 12.00m, AvailableQuantity = 500, CreatedDate = now }
        );
    }
}

