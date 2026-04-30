using Entravel.EF.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Entravel.EF.Infrastructure.Persistence.Configurations;

internal sealed class InventoryEntityConfiguration : IEntityTypeConfiguration<InventoryEntity>
{
    public void Configure(EntityTypeBuilder<InventoryEntity> builder)
    {
        builder.ToTable("Inventory");

        builder.HasKey(inventory => inventory.Id);

        builder.Property(inventory => inventory.Id)
            .ValueGeneratedNever();

        builder.Property(inventory => inventory.CreatedDate)
            .IsRequired();

        builder.Property(inventory => inventory.UpdatedDate)
            .IsRequired(false);

        builder.Property(inventory => inventory.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(inventory => inventory.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(inventory => inventory.AvailableQuantity)
            .IsRequired();
    }
}
