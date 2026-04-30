using AutoMapper;
using Entravel.Data.Repositories;
using Entravel.Domain.Outbox;
using Entravel.EF.DbContext;
using Entravel.EF.Infrastructure.Persistence.Entities;

namespace Entravel.EF.Repositories;

public sealed class OutboxMessageRepository(AppDbContext appDbContext, IMapper mapper) : IOutboxMessageRepository
{
    public async Task AddAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken)
    {
        var entity = mapper.Map<OutboxMessageEntity>(outboxMessage);
        await appDbContext.OutboxMessages.AddAsync(entity, cancellationToken);
    }
}
