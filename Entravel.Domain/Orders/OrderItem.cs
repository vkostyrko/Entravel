using Entravel.Domain.Common;

namespace Entravel.Domain.Orders;

public sealed class OrderItem : BaseDomainModel
{
    public Guid OrderId { get; set; }
    public Guid InventoryId { get; set; }
    public int Quantity { get; set; }
}
