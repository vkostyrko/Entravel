namespace Entravel.Rmq;

public sealed class RabbitMqMessageEnvelope
{
    public required string ConfiguredMessageType { get; init; }
    public string? PropertyMessageType { get; init; }
    public required string RoutingKey { get; init; }
    public string? RabbitMqMessageId { get; init; }
    public required ReadOnlyMemory<byte> Body { get; init; }
}
