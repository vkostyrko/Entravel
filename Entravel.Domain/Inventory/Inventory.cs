using Entravel.Domain.Common;

namespace Entravel.Domain.Inventory;

public sealed class Inventory : BaseEntity
{
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public int AvailableQuantity { get; set; }
}

