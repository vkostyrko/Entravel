using Entravel.Domain.Outbox;

namespace Entravel.Data.Outbox;

public sealed record OutboxMessageToPublish(
    Guid Id,
    string Type,
    string Payload,
    OutboxMessageStatus Status,
    int RetryCount,
    DateTime CreatedDate,
    DateTime? UpdatedDate);

