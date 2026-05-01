namespace Entravel.Data.Repositories;

public interface IOrderProcessingRepository
{
    Task<OrderProcessingOutcome> ProcessOrderSubmittedAsync(Guid orderId, CancellationToken cancellationToken);
}
