using System.Text.Json;
using Entravel.Application.Orders.OrderSubmitted;
using Entravel.Contracts.Integration.Orders;
using Entravel.Data.Repositories;
using Entravel.OrderProcessingWorker.Observability;
using Entravel.Rmq;

namespace Entravel.OrderProcessingWorker.Consumers;

public sealed class OrderProcessingRabbitMqDispatcher(
    OrderSubmittedMessageHandler orderSubmittedHandler,
    ILogger<OrderProcessingRabbitMqDispatcher> logger)
    : IRabbitMqMessageDispatcher
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<RabbitMqDispatchResult> DispatchAsync(RabbitMqMessageEnvelope envelope, CancellationToken cancellationToken)
    {
        var messageType = !string.IsNullOrWhiteSpace(envelope.PropertyMessageType)
            ? envelope.PropertyMessageType!
            : envelope.ConfiguredMessageType;

        if (string.IsNullOrWhiteSpace(messageType))
        {
            logger.LogError("Message type could not be resolved (no AMQP type header and no subscription MessageType).");
            return RabbitMqDispatchResult.NackDiscard;
        }

        try
        {
            if (!OrderSubmittedMessageHandler.CanHandle(messageType))
            {
                logger.LogError("Unknown or unsupported message type: {MessageType}", messageType);
                return RabbitMqDispatchResult.NackDiscard;
            }

            OrderSubmittedMessage? message;
            try
            {
                message = JsonSerializer.Deserialize<OrderSubmittedMessage>(envelope.Body.Span, JsonOptions);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Invalid JSON for OrderSubmitted payload.");
                return RabbitMqDispatchResult.NackDiscard;
            }

            if (message is null)
            {
                logger.LogError("OrderSubmitted payload deserialized to null.");
                return RabbitMqDispatchResult.NackDiscard;
            }

            var outcome = await orderSubmittedHandler.HandleAsync(message, cancellationToken);

            if (outcome == OrderProcessingOutcome.Success)
            {
                OrderProcessingMetrics.RecordProcessedOrder(message.OrderId, logger);
            }

            return outcome switch
            {
                OrderProcessingOutcome.Success => RabbitMqDispatchResult.Ack,
                OrderProcessingOutcome.AlreadyProcessed => RabbitMqDispatchResult.Ack,
                OrderProcessingOutcome.NotFound => RabbitMqDispatchResult.NackDiscard,
                OrderProcessingOutcome.PermanentFailure => RabbitMqDispatchResult.NackDiscard,
                OrderProcessingOutcome.ConcurrentProcessing => RabbitMqDispatchResult.NackRequeue,
                _ => RabbitMqDispatchResult.NackRequeue
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Transient error while handling message type {MessageType}", messageType);
            return RabbitMqDispatchResult.NackRequeue;
        }
    }
}

