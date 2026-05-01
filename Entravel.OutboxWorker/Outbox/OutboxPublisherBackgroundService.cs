using Entravel.Data.Outbox;
using Entravel.Domain.Outbox;
using Entravel.Rmq;
using Microsoft.Extensions.Options;

namespace Entravel.OutboxWorker.Outbox;

public sealed class OutboxPublisherBackgroundService(
    IServiceScopeFactory scopeFactory,
    IRabbitMqPublisher publisher,
    IOptions<OutboxPublisherOptions> options,
    ILogger<OutboxPublisherBackgroundService> logger)
    : BackgroundService
{
    private readonly OutboxPublisherOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pollDelay = TimeSpan.FromSeconds(Math.Max(1, _options.PollIntervalSeconds));
        var processingTimeout = TimeSpan.FromSeconds(Math.Max(1, _options.ProcessingTimeoutSeconds));
        var maxParallelism = Math.Max(1, _options.MaxParallelism);

        using var semaphore = new SemaphoreSlim(maxParallelism, maxParallelism);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var utcNow = DateTime.UtcNow;

                IReadOnlyList<OutboxMessageToPublish> batch;
                await using (var claimScope = scopeFactory.CreateAsyncScope())
                {
                    var outboxRepository = claimScope.ServiceProvider.GetRequiredService<IOutboxProcessingRepository>();
                    batch = await outboxRepository.ClaimBatchAsync(
                        batchSize: _options.BatchSize,
                        maxRetryCount: _options.MaxRetryCount,
                        processingTimeout: processingTimeout,
                        utcNow: utcNow,
                        cancellationToken: stoppingToken);
                }

                if (batch.Count == 0)
                {
                    await Task.Delay(pollDelay, stoppingToken);
                    continue;
                }

                var tasks = batch.Select(async msg =>
                {
                    await semaphore.WaitAsync(stoppingToken);
                    try
                    {
                        await PublishOneAsync(msg, stoppingToken);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                await Task.WhenAll(tasks);
                await Task.Delay(pollDelay, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox publisher loop error");
                await Task.Delay(pollDelay, stoppingToken);
            }
        }
    }

    private async Task PublishOneAsync(OutboxMessageToPublish message, CancellationToken cancellationToken)
    {
        try
        {
            if (message.Status != OutboxMessageStatus.Processing)
            {
                logger.LogWarning("Skipping message {MessageId} because status is {Status}", message.Id, message.Status);
                return;
            }

            await publisher.PublishJsonAsync(message.Id, message.Type, message.Payload, cancellationToken);
            await using var scope = scopeFactory.CreateAsyncScope();
            var repo = scope.ServiceProvider.GetRequiredService<IOutboxProcessingRepository>();
            await repo.MarkSentAsync(message.Id, DateTime.UtcNow, cancellationToken);

            logger.LogInformation("Published outbox message {MessageId} ({Type})", message.Id, message.Type);
        }
        catch (InvalidOperationException ex) when (ex.Message.StartsWith("Unknown message type:", StringComparison.Ordinal))
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var repo = scope.ServiceProvider.GetRequiredService<IOutboxProcessingRepository>();
            await repo.MarkFailedAsync(message.Id, ex.Message, _options.MaxRetryCount, DateTime.UtcNow, cancellationToken);
            logger.LogWarning(ex, "Unknown outbox message type for {MessageId}", message.Id);
        }
        catch (Exception ex)
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var repo = scope.ServiceProvider.GetRequiredService<IOutboxProcessingRepository>();
            await repo.MarkFailedAsync(message.Id, ex.ToString(), _options.MaxRetryCount, DateTime.UtcNow, cancellationToken);
            logger.LogWarning(ex, "Failed to publish outbox message {MessageId}", message.Id);
        }
    }
}
