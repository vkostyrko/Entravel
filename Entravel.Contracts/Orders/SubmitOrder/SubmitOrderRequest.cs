namespace Entravel.Contracts.Orders.SubmitOrder;

public sealed record SubmitOrderRequest(
    string CustomerId,
    IReadOnlyList<OrderItemRequest> Items);

