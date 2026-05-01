using Entravel.EF.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Entravel.EF.Infrastructure.Persistence.Configurations;

internal sealed class CustomerEntityConfiguration : IEntityTypeConfiguration<CustomerEntity>
{
    public void Configure(EntityTypeBuilder<CustomerEntity> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(customer => customer.Id);

        builder.Property(customer => customer.Id)
            .ValueGeneratedNever();

        builder.Property(customer => customer.CreatedDate)
            .IsRequired();

        builder.Property(customer => customer.UpdatedDate)
            .IsRequired(false);

        builder.Property(customer => customer.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(customer => customer.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.HasIndex(customer => customer.Email)
            .IsUnique();
    }
}

