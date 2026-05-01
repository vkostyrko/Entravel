using Entravel.Data.Outbox;
using Entravel.Domain.Outbox;
using Entravel.EF.DbContext;
using Entravel.EF.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Entravel.EF.Outbox;

public sealed class OutboxProcessingRepository(AppDbContext db) : IOutboxProcessingRepository
{
    public async Task<IReadOnlyList<OutboxMessageToPublish>> ClaimBatchAsync(
        int batchSize,
        int maxRetryCount,
        TimeSpan processingTimeout,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        var processingCutoff = utcNow.Subtract(processingTimeout);
        var newStatus = OutboxMessageStatus.New.ToString();
        var failedStatus = OutboxMessageStatus.Failed.ToString();
        var processingStatus = OutboxMessageStatus.Processing.ToString();

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

        var rows = await db.Set<OutboxMessageEntity>()
            .FromSqlInterpolated($@"
SELECT *
FROM ""OutboxMessages""
WHERE
    (""Status"" = {newStatus})
    OR (""Status"" = {failedStatus} AND ""RetryCount"" < {maxRetryCount})
    OR (""Status"" = {processingStatus} AND ""UpdatedDate"" IS NOT NULL AND ""UpdatedDate"" < {processingCutoff})
ORDER BY ""CreatedDate""
LIMIT {batchSize}
FOR UPDATE SKIP LOCKED")
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            await tx.CommitAsync(cancellationToken);
            return [];
        }

        foreach (var row in rows)
        {
            row.Status = OutboxMessageStatus.Processing;
            row.UpdatedDate = utcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        return rows
            .Select(r => new OutboxMessageToPublish(
                r.Id,
                r.Type,
                r.Payload,
                r.Status,
                r.RetryCount,
                r.CreatedDate,
                r.UpdatedDate))
            .ToArray();
    }

    public Task MarkSentAsync(Guid id, DateTime utcNow, CancellationToken cancellationToken) =>
        db.Set<OutboxMessageEntity>()
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(updates => updates
                .SetProperty(x => x.Status, OutboxMessageStatus.Sent)
                .SetProperty(x => x.SentDate, utcNow)
                .SetProperty(x => x.LastError, (string?)null)
                .SetProperty(x => x.UpdatedDate, utcNow), cancellationToken);

    public async Task MarkFailedAsync(Guid id, string lastError, int maxRetryCount, DateTime utcNow, CancellationToken cancellationToken)
    {
        var entity = await db.Set<OutboxMessageEntity>()
            .Where(x => x.Id == id)
            .Select(x => new { x.Id, x.RetryCount })
            .SingleOrDefaultAsync(cancellationToken);

        if (entity is null)
        {
            return;
        }

        var newRetryCount = entity.RetryCount + 1;
        var nextStatus = newRetryCount < maxRetryCount ? OutboxMessageStatus.New : OutboxMessageStatus.Failed;

        await db.Set<OutboxMessageEntity>()
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(updates => updates
                .SetProperty(x => x.Status, nextStatus)
                .SetProperty(x => x.RetryCount, newRetryCount)
                .SetProperty(x => x.LastError, lastError)
                .SetProperty(x => x.UpdatedDate, utcNow), cancellationToken);
    }
}

