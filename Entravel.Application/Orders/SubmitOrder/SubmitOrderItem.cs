namespace Entravel.Application.Orders.SubmitOrder;

public sealed record SubmitOrderItem(
    Guid InventoryId,
    int Quantity);

