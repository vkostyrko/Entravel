namespace Entravel.Data.Queries;

public interface IOrderReadRepository
{
    Task<OrderReadModel?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken);
}

