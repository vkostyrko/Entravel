using Entravel.Data.Queries;
using Entravel.EF.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Entravel.EF.Queries;

public sealed class OrderReadRepository(AppDbContext db) : IOrderReadRepository
{
    public async Task<OrderReadModel?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .SingleOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order is null)
        {
            return null;
        }

        var items = order.Items
            .Select(i => new OrderItemReadModel(i.InventoryId, i.Quantity))
            .ToList();

        return new OrderReadModel(
            Id: order.Id,
            CustomerId: order.CustomerId,
            Status: order.Status,
            TotalAmount: order.TotalAmount,
            Discount: order.Discount,
            FinalAmount: order.FinalAmount,
            CreatedDate: order.CreatedDate,
            UpdatedDate: order.UpdatedDate,
            Items: items);
    }
}

