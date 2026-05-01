using Entravel.Domain.Common;

namespace Entravel.Domain.Inventory;

public sealed class Inventory : BaseDomainModel
{
    private Inventory()
    {
    }

    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int AvailableQuantity { get; private set; }

    public static Inventory Create(Guid id, string name, decimal price, int availableQuantity, DateTime utcNow)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Inventory id is required.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Inventory name is required.", nameof(name));
        }

        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "Inventory price cannot be negative.");
        }

        if (availableQuantity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(availableQuantity), "Inventory available quantity cannot be negative.");
        }

        return new Inventory
        {
            Id = id,
            Name = name.Trim(),
            Price = price,
            AvailableQuantity = availableQuantity,
            CreatedDate = utcNow,
            UpdatedDate = null
        };
    }

    public static Inventory Rehydrate(Guid id, string name, decimal price, int availableQuantity, DateTime createdDate, DateTime? updatedDate) =>
    new()
    {
        Id = id,
        Name = name,
        Price = price,
        AvailableQuantity = availableQuantity,
        CreatedDate = createdDate,
        UpdatedDate = updatedDate
    };
}

