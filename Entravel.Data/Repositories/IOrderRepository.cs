using Entravel.Domain.Orders;

namespace Entravel.Data.Repositories;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken cancellationToken);
}

