namespace Entravel.Contracts.Orders.SubmitOrder;

public sealed record OrderItemRequest(
    string InventoryId,
    int Quantity);

