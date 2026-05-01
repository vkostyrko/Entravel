namespace Entravel.Contracts.Orders.SubmitOrder;

public sealed record SubmitOrderRequest(
    Guid CustomerId,
    IReadOnlyList<OrderItemRequest> Items,
    decimal TotalAmount,
    decimal Discount);

