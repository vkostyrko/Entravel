using Entravel.Domain.Common;
using InventoryEntity = Entravel.Domain.Inventory.Inventory;

namespace Entravel.Domain.Orders;

public sealed class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public required Order Order { get; set; }

    public Guid InventoryId { get; set; }
    public required InventoryEntity Inventory { get; set; }

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

