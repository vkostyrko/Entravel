namespace Entravel.EF.Infrastructure.Persistence.Entities;

internal sealed class InventoryEntity : PersistenceEntityBase
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int AvailableQuantity { get; set; }
    public ICollection<OrderItemEntity> OrderItems { get; set; } = new List<OrderItemEntity>();
}
