using Entravel.Domain.Orders;

namespace Entravel.EF.Infrastructure.Persistence.Entities;

internal sealed class OrderEntity : PersistenceEntityBase
{
    public Guid CustomerId { get; set; }
    public CustomerEntity? Customer { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Discount { get; set; }
    public decimal? FinalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public ICollection<OrderItemEntity> Items { get; set; } = new List<OrderItemEntity>();
}
