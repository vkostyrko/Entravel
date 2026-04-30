using Entravel.Domain.Common;

namespace Entravel.Domain.Outbox;

public sealed class OutboxMessage : BaseEntity
{
    public required string Type { get; set; }
    public required string Payload { get; set; }
    public OutboxMessageStatus Status { get; set; }
    public int RetryCount { get; set; }
    public string? LastError { get; set; }
    public DateTime? SentDate { get; set; }
}

