using Entravel.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Entravel.EF.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(orderItem => orderItem.Id);

        builder.Property(orderItem => orderItem.Id)
            .ValueGeneratedNever();

        builder.Property(orderItem => orderItem.CreatedDate)
            .IsRequired();

        builder.Property(orderItem => orderItem.UpdatedDate)
            .IsRequired(false);

        builder.Property(orderItem => orderItem.OrderId)
            .IsRequired();

        builder.Property(orderItem => orderItem.InventoryId)
            .IsRequired();

        builder.Property(orderItem => orderItem.Quantity)
            .IsRequired();

        builder.Property(orderItem => orderItem.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasOne(orderItem => orderItem.Order)
            .WithMany(order => order.Items)
            .HasForeignKey(orderItem => orderItem.OrderId);

        builder.HasOne(orderItem => orderItem.Inventory)
            .WithMany()
            .HasForeignKey(orderItem => orderItem.InventoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

