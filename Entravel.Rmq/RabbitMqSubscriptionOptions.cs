namespace Entravel.Rmq;

public sealed class RabbitMqSubscriptionOptions
{
    public string QueueName { get; init; } = string.Empty;
    public string ExchangeName { get; init; } = string.Empty;
    public string ExchangeType { get; init; } = "topic";
    public string RoutingKey { get; init; } = string.Empty;
    public string MessageType { get; init; } = string.Empty;
}
