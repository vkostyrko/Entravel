using Entravel.Domain.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Entravel.EF.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    private const int TypeMaxLength = 256;
    private const int PayloadMaxLength = 8000;
    private const int LastErrorMaxLength = 4000;

    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(outboxMessage => outboxMessage.Id);

        builder.Property(outboxMessage => outboxMessage.Id)
            .ValueGeneratedNever();

        builder.Property(outboxMessage => outboxMessage.CreatedDate)
            .IsRequired();

        builder.Property(outboxMessage => outboxMessage.UpdatedDate)
            .IsRequired(false);

        builder.Property(outboxMessage => outboxMessage.Type)
            .HasMaxLength(TypeMaxLength)
            .IsRequired();

        builder.Property(outboxMessage => outboxMessage.Payload)
            .HasMaxLength(PayloadMaxLength)
            .IsRequired();

        builder.Property(outboxMessage => outboxMessage.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(outboxMessage => outboxMessage.RetryCount)
            .IsRequired();

        builder.Property(outboxMessage => outboxMessage.LastError)
            .HasMaxLength(LastErrorMaxLength)
            .IsRequired(false);

        builder.Property(outboxMessage => outboxMessage.SentDate)
            .IsRequired(false);

        builder.HasIndex(outboxMessage => outboxMessage.Status);
    }
}

