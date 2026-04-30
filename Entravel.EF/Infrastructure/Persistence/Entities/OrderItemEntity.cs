namespace Entravel.EF.Infrastructure.Persistence.Entities;

internal sealed class OrderItemEntity : PersistenceEntityBase
{
    public Guid OrderId { get; set; }
    public OrderEntity Order { get; set; } = null!;

    public Guid InventoryId { get; set; }
    public InventoryEntity? Inventory { get; set; }

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
