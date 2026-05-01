using Entravel.Data.Repositories;
using Entravel.Domain.Orders;
using Entravel.EF.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Entravel.EF.Repositories;

public sealed class OrderProcessingRepository(AppDbContext db) : IOrderProcessingRepository
{
    public async Task<OrderProcessingOutcome> ProcessOrderSubmittedAsync(Guid orderId, CancellationToken cancellationToken)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var entity = await db.Orders
            .FromSqlInterpolated($@"SELECT * FROM ""Orders"" WHERE ""Id"" = {orderId} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);

        if (entity is null)
        {
            return OrderProcessingOutcome.NotFound;
        }

        if (entity.Status == OrderStatus.Processed)
        {
            return OrderProcessingOutcome.AlreadyProcessed;
        }

        if (entity.Status == OrderStatus.Processing)
        {
            return OrderProcessingOutcome.ConcurrentProcessing;
        }

        if (entity.Status == OrderStatus.Failed)
        {
            return OrderProcessingOutcome.PermanentFailure;
        }

        var now = DateTime.UtcNow;
        var order = Order.Rehydrate(
            id: entity.Id,
            customerId: entity.CustomerId,
            totalAmount: entity.TotalAmount,
            discount: entity.Discount,
            finalAmount: entity.FinalAmount,
            status: entity.Status,
            createdDate: entity.CreatedDate,
            updatedDate: entity.UpdatedDate);

        order.StartProcessing(now);

        await Task.Delay(TimeSpan.FromMilliseconds(75), cancellationToken);

        order.Process(DateTime.UtcNow);

        entity.Status = order.Status;
        entity.UpdatedDate = order.UpdatedDate;
        entity.FinalAmount = order.FinalAmount;

        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        return OrderProcessingOutcome.Success;
    }
}
