namespace Entravel.Rmq;

public sealed class RabbitMqOptions
{
    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 5672;
    public string Username { get; init; } = "guest";
    public string Password { get; init; } = "guest";
    public string Exchange { get; init; } = "entravel.events";
    public string? ExchangeName { get; init; }
    public int PrefetchCount { get; init; } = 10;
    public List<RabbitMqSubscriptionOptions> Subscriptions { get; init; } = [];

    public string ResolvedExchangeName =>
        string.IsNullOrWhiteSpace(ExchangeName) ? Exchange : ExchangeName!;
}
