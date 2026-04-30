using MediatR;

namespace Entravel.Application.Orders.SubmitOrder;

public sealed record SubmitOrderCommand(
    Guid CustomerId,
    IReadOnlyList<SubmitOrderItem> Items,
    decimal TotalAmount) : IRequest<SubmitOrderResult>;
