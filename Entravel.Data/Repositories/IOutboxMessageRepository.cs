using Entravel.Domain.Outbox;

namespace Entravel.Data.Repositories;

public interface IOutboxMessageRepository
{
    Task AddAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken);
}

