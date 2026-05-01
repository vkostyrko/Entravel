using System.Text.Json;
using Entravel.Common;
using Entravel.Contracts.Integration.Orders;
using Entravel.OrderProcessingWorker.Consumers.Commands;
using Entravel.Rmq;

namespace Entravel.OrderProcessingWorker.Consumers.Factories;

public sealed class OrderSubmittedCommandFactory : IRabbitMqCommandFactory
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public string MessageType => MessageTypes.OrderSubmitted.ToString();

    public object Create(ReadOnlyMemory<byte> body, RabbitMqMessageEnvelope envelope)
    {
        OrderSubmittedMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<OrderSubmittedMessage>(body.Span, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidRabbitMqMessageException($"Invalid JSON for message type {MessageType}.", ex);
        }

        if (message is null)
        {
            throw new InvalidRabbitMqMessageException($"Payload for message type {MessageType} deserialized to null.");
        }

        return new ProcessSubmittedOrderCommand(
            OrderId: message.OrderId,
            MessageId: envelope.RabbitMqMessageId);
    }
}

