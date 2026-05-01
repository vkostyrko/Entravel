using Entravel.EF.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Entravel.EF.Infrastructure.Persistence.Configurations;

internal sealed class OrderEntityConfiguration : IEntityTypeConfiguration<OrderEntity>
{
    public void Configure(EntityTypeBuilder<OrderEntity> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(order => order.Id);

        builder.Property(order => order.Id)
            .ValueGeneratedNever();

        builder.Property(order => order.CreatedDate)
            .IsRequired();

        builder.Property(order => order.UpdatedDate)
            .IsRequired(false);

        builder.Property(order => order.CustomerId)
            .IsRequired();

        builder.HasOne(order => order.Customer)
            .WithMany()
            .HasForeignKey(order => order.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(order => order.TotalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(order => order.Discount)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(order => order.FinalAmount)
            .HasPrecision(18, 2)
            .IsRequired(false);

        builder.Property(order => order.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasMany(order => order.Items)
            .WithOne(orderItem => orderItem.Order)
            .HasForeignKey(orderItem => orderItem.OrderId);

        builder.HasIndex(order => order.Status);
        builder.HasIndex(order => order.CustomerId);
    }
}
