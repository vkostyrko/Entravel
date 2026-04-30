using MediatR;

namespace Entravel.Application.Orders.SubmitOrder;

public sealed record SubmitOrderCommand(
    string CustomerId,
    IReadOnlyList<SubmitOrderItem> Items) : IRequest<SubmitOrderResult>;
