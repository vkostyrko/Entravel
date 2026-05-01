namespace Entravel.Data.Queries;

public sealed record OrderItemReadModel(
    Guid InventoryId,
    int Quantity);

