namespace Entravel.Application.Orders.SubmitOrder;

public sealed record SubmitOrderItem(
    string InventoryId,
    int Quantity);

