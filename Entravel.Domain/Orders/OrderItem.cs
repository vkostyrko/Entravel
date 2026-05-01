using Entravel.Domain.Common;

namespace Entravel.Domain.Orders;

public sealed class OrderItem : BaseDomainModel
{
    private OrderItem()
    {
    }

    public Guid OrderId { get; private set; }
    public Guid InventoryId { get; private set; }
    public int Quantity { get; private set; }

    public static OrderItem Create(Guid id, Guid orderId, Guid inventoryId, int quantity, DateTime utcNow)
    {
        if (id == Guid.Empty) throw new ArgumentException("OrderItem id is required.", nameof(id));
        if (orderId == Guid.Empty) throw new ArgumentException("OrderId is required.", nameof(orderId));
        if (inventoryId == Guid.Empty) throw new ArgumentException("InventoryId is required.", nameof(inventoryId));
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than 0.");

        return new OrderItem
        {
            Id = id,
            OrderId = orderId,
            InventoryId = inventoryId,
            Quantity = quantity,
            CreatedDate = utcNow,
            UpdatedDate = null
        };
    }

    public static OrderItem Rehydrate(Guid id, Guid orderId, Guid inventoryId, int quantity, DateTime createdDate, DateTime? updatedDate)
    {
        return new OrderItem
        {
            Id = id,
            OrderId = orderId,
            InventoryId = inventoryId,
            Quantity = quantity,
            CreatedDate = createdDate,
            UpdatedDate = updatedDate
        };
    }
}
