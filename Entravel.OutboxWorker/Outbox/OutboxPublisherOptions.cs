namespace Entravel.OutboxWorker.Outbox;

public sealed class OutboxPublisherOptions
{
    public int PollIntervalSeconds { get; init; } = 5;
    public int BatchSize { get; init; } = 10;
    public int MaxRetryCount { get; init; } = 5;
    public int ProcessingTimeoutSeconds { get; init; } = 300;
    public int MaxParallelism { get; init; } = 4;
}

