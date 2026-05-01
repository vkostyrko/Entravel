using Entravel.Data.Queries;
using MediatR;

namespace Entravel.Application.Orders.GetOrderById;

public sealed class GetOrderByIdQueryHandler(IOrderReadRepository orders)
    : IRequestHandler<GetOrderByIdQuery, OrderReadModel?>
{
    public Task<OrderReadModel?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken) =>
        orders.GetByIdAsync(request.OrderId, cancellationToken);
}

