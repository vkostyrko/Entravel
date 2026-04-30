namespace Entravel.Contracts.Orders.SubmitOrder;

public sealed record OrderItemRequest(
    Guid InventoryId,
    int Quantity);

