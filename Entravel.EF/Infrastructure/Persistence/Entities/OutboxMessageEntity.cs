using Entravel.Domain.Outbox;

namespace Entravel.EF.Infrastructure.Persistence.Entities;

internal sealed class OutboxMessageEntity : PersistenceEntityBase
{
    public string Type { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public OutboxMessageStatus Status { get; set; }
    public int RetryCount { get; set; }
    public string? LastError { get; set; }
    public DateTime? SentDate { get; set; }
}
