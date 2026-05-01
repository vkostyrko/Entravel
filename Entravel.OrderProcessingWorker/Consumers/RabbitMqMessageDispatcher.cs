using Entravel.OrderProcessingWorker.Consumers.Factories;
using Entravel.Rmq;
using MediatR;
using System.Net.Sockets;

namespace Entravel.OrderProcessingWorker.Consumers;

public sealed class RabbitMqMessageDispatcher(
    IEnumerable<IRabbitMqCommandFactory> factories,
    IMediator mediator,
    ILogger<RabbitMqMessageDispatcher> logger)
    : IRabbitMqMessageDispatcher
{
    private readonly IReadOnlyDictionary<string, IRabbitMqCommandFactory> _factoriesByType =
        BuildFactories(factories);

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

        if (!_factoriesByType.TryGetValue(messageType, out var factory))
        {
            logger.LogError("No RabbitMQ command factory registered for message type {MessageType}", messageType);
            return RabbitMqDispatchResult.NackDiscard;
        }

        object command;
        try
        {
            command = factory.Create(envelope.Body, envelope);
        }
        catch (InvalidRabbitMqMessageException ex)
        {
            logger.LogError(ex,
                "Invalid message payload. messageType={MessageType} messageId={MessageId}",
                messageType,
                envelope.RabbitMqMessageId);
            return RabbitMqDispatchResult.NackDiscard;
        }

        if (command is IRequest<RabbitMqDispatchResult> typedCommand)
        {
            try
            {
                return await mediator.Send(typedCommand, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex) when (IsTransient(ex))
            {
                logger.LogWarning(ex,
                    "Transient error while dispatching RabbitMQ message via MediatR. messageType={MessageType} messageId={MessageId}",
                    messageType,
                    envelope.RabbitMqMessageId);
                return RabbitMqDispatchResult.NackRequeue;
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Unexpected error while dispatching RabbitMQ message via MediatR. messageType={MessageType} messageId={MessageId}",
                    messageType,
                    envelope.RabbitMqMessageId);
                return RabbitMqDispatchResult.NackDiscard;
            }
        }

        logger.LogError(
            "Command factory returned unsupported command type. commandType={CommandType} messageType={MessageType} messageId={MessageId}",
            command.GetType().FullName,
            messageType,
            envelope.RabbitMqMessageId);
        return RabbitMqDispatchResult.NackDiscard;
    }

    private static IReadOnlyDictionary<string, IRabbitMqCommandFactory> BuildFactories(IEnumerable<IRabbitMqCommandFactory> factories)
    {
        var any = false;
        var dict = new Dictionary<string, IRabbitMqCommandFactory>(StringComparer.Ordinal);
        foreach (var factory in factories)
        {
            any = true;
            if (string.IsNullOrWhiteSpace(factory.MessageType))
            {
                throw new InvalidOperationException("RabbitMQ command factory has empty MessageType.");
            }

            if (!dict.TryAdd(factory.MessageType, factory))
            {
                throw new InvalidOperationException($"Duplicate RabbitMQ command factory for MessageType '{factory.MessageType}'.");
            }
        }

        if (!any)
        {
            throw new InvalidOperationException("No RabbitMQ command factories are registered. Register at least one IRabbitMqCommandFactory.");
        }

        return dict;
    }

    private static bool IsTransient(Exception ex) =>
        ex is TimeoutException
        || ex is IOException
        || ex is SocketException;
}

