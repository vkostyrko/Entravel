using Entravel.EF.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Entravel.EF.DbContext;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : Microsoft.EntityFrameworkCore.DbContext(options)
{
    internal DbSet<OrderEntity> Orders => Set<OrderEntity>();
    internal DbSet<OrderItemEntity> OrderItems => Set<OrderItemEntity>();
    internal DbSet<InventoryEntity> Inventory => Set<InventoryEntity>();
    internal DbSet<CustomerEntity> Customers => Set<CustomerEntity>();
    internal DbSet<OutboxMessageEntity> OutboxMessages => Set<OutboxMessageEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
