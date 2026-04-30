using Entravel.Domain.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Entravel.EF.Configurations;

public sealed class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
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

