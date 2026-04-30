namespace Entravel.Contracts.Orders.SubmitOrder;

public sealed record OrderResponse(
    string InventoryId,
    int Quantity);

