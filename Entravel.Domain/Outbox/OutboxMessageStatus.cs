namespace Entravel.Domain.Outbox;

public enum OutboxMessageStatus
{
    New,
    Processing,
    Sent,
    Failed
}

