using AutoMapper;
using Entravel.Data.Repositories;
using Entravel.Domain.Orders;
using Entravel.EF.DbContext;
using Entravel.EF.Infrastructure.Persistence.Entities;

namespace Entravel.EF.Repositories;

public sealed class OrderRepository(AppDbContext appDbContext, IMapper mapper) : IOrderRepository
{
    public async Task AddAsync(Order order, CancellationToken cancellationToken)
    {
        var orderEntity = mapper.Map<OrderEntity>(order);
        await appDbContext.Orders.AddAsync(orderEntity, cancellationToken);
    }
}
