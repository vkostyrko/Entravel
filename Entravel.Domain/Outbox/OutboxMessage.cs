using Entravel.Domain.Common;

namespace Entravel.Domain.Outbox;

public sealed class OutboxMessage : BaseDomainModel
{
    private OutboxMessage()
    {
    }

    public required string Type { get; set; }
    public required string Payload { get; set; }
    public OutboxMessageStatus Status { get; set; }
    public int RetryCount { get; set; }
    public string? LastError { get; set; }
    public DateTime? SentDate { get; set; }

    public static OutboxMessage CreateNew(Guid id, string type, string payload, DateTime utcNow)
    {
        if (id == Guid.Empty) throw new ArgumentException("Outbox message id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("Outbox message type is required.", nameof(type));
        if (string.IsNullOrWhiteSpace(payload)) throw new ArgumentException("Outbox message payload is required.", nameof(payload));

        return new OutboxMessage
        {
            Id = id,
            Type = type.Trim(),
            Payload = payload,
            Status = OutboxMessageStatus.New,
            RetryCount = 0,
            LastError = null,
            SentDate = null,
            CreatedDate = utcNow,
            UpdatedDate = null
        };
    }
}

