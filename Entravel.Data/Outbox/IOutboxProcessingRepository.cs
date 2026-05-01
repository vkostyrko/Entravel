namespace Entravel.Data.Outbox;

public interface IOutboxProcessingRepository
{
    Task<IReadOnlyList<OutboxMessageToPublish>> ClaimBatchAsync(
        int batchSize,
        int maxRetryCount,
        TimeSpan processingTimeout,
        DateTime utcNow,
        CancellationToken cancellationToken);

    Task MarkSentAsync(Guid id, DateTime utcNow, CancellationToken cancellationToken);

    Task MarkFailedAsync(
        Guid id,
        string lastError,
        int maxRetryCount,
        DateTime utcNow,
        CancellationToken cancellationToken);
}

