using Entravel.Domain.Orders;

namespace Entravel.Data.Queries;

public sealed record OrderReadModel(
    Guid Id,
    Guid CustomerId,
    OrderStatus Status,
    decimal TotalAmount,
    decimal Discount,
    decimal? FinalAmount,
    DateTime CreatedDate,
    DateTime? UpdatedDate,
    IReadOnlyList<OrderItemReadModel> Items);

