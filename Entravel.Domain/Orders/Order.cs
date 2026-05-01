using Entravel.Domain.Common;

namespace Entravel.Domain.Orders;

public sealed class Order : BaseDomainModel
{
    private readonly List<OrderItem> _items = [];

    private Order()
    {
    }

    public Guid CustomerId { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal Discount { get; private set; }
    public decimal? FinalAmount { get; private set; }
    public OrderStatus Status { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items;

    public static Order CreateSubmitted(Guid id, Guid customerId, decimal totalAmount, decimal discount, DateTime utcNow)
    {
        if (id == Guid.Empty) throw new ArgumentException("Order id is required.", nameof(id));
        if (customerId == Guid.Empty) throw new ArgumentException("CustomerId is required.", nameof(customerId));
        if (totalAmount <= 0) throw new ArgumentOutOfRangeException(nameof(totalAmount), "TotalAmount must be greater than 0.");
        if (discount is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(discount), "Discount must be between 0 and 100.");

        return new Order
        {
            Id = id,
            CustomerId = customerId,
            TotalAmount = totalAmount,
            Discount = discount,
            Status = OrderStatus.Pending,
            CreatedDate = utcNow,
            UpdatedDate = null,
            FinalAmount = null
        };
    }

    public static Order Rehydrate(
        Guid id,
        Guid customerId,
        decimal totalAmount,
        decimal discount,
        decimal? finalAmount,
        OrderStatus status,
        DateTime createdDate,
        DateTime? updatedDate)
    {
        return new Order
        {
            Id = id,
            CustomerId = customerId,
            TotalAmount = totalAmount,
            Discount = discount,
            FinalAmount = finalAmount,
            Status = status,
            CreatedDate = createdDate,
            UpdatedDate = updatedDate
        };
    }

    public void AddItem(OrderItem item)
    {
        if (item is null) throw new ArgumentNullException(nameof(item));
        _items.Add(item);
    }

    public void StartProcessing(DateTime utcNow)
    {
        if (Status == OrderStatus.Processed)
        {
            return;
        }

        if (Status == OrderStatus.Processing)
        {
            return;
        }

        Status = OrderStatus.Processing;
        UpdatedDate = utcNow;
    }

    public void Process(DateTime utcNow)
    {
        if (Status == OrderStatus.Processed)
        {
            return;
        }

        if (Discount is < 0 or > 100)
        {
            throw new InvalidOperationException("Discount must be between 0 and 100.");
        }

        var discountAmount = TotalAmount * Discount / 100m;
        FinalAmount = TotalAmount - discountAmount;

        Status = OrderStatus.Processed;
        UpdatedDate = utcNow;
    }
}

