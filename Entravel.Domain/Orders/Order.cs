using Entravel.Domain.Common;

namespace Entravel.Domain.Orders;

public sealed class Order : BaseEntity
{
    public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

