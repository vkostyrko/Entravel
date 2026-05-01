using Entravel.Data.Repositories;
using Entravel.OrderProcessingWorker.Observability;
using Entravel.Rmq;
using MediatR;
using System.Net.Sockets;

namespace Entravel.OrderProcessingWorker.Consumers.Commands;

public sealed class ProcessSubmittedOrderCommandHandler(
    IOrderProcessingRepository orderProcessingRepository,
    ILogger<ProcessSubmittedOrderCommandHandler> logger)
    : IRequestHandler<ProcessSubmittedOrderCommand, RabbitMqDispatchResult>
{
    public async Task<RabbitMqDispatchResult> Handle(ProcessSubmittedOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.OrderId == Guid.Empty)
        {
            logger.LogWarning("OrderSubmitted command has empty OrderId. MessageId={MessageId}", request.MessageId);
            return RabbitMqDispatchResult.NackDiscard;
        }

        OrderProcessingOutcome outcome;
        try
        {
            outcome = await orderProcessingRepository.ProcessOrderSubmittedAsync(request.OrderId, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (IsTransient(ex))
        {
            logger.LogWarning(ex,
                "Transient error while processing order {OrderId}. MessageId={MessageId}",
                request.OrderId,
                request.MessageId);
            return RabbitMqDispatchResult.NackRequeue;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Unexpected error while processing order {OrderId}. MessageId={MessageId}",
                request.OrderId,
                request.MessageId);
            return RabbitMqDispatchResult.NackDiscard;
        }

        if (outcome == OrderProcessingOutcome.Success)
        {
            OrderProcessingMetrics.RecordProcessedOrder(request.OrderId, request.MessageId, logger);
        }
        else if (outcome == OrderProcessingOutcome.AlreadyProcessed)
        {
            logger.LogInformation(
                "Order already processed. OrderId={OrderId} MessageId={MessageId}",
                request.OrderId,
                request.MessageId);
        }
        else if (outcome == OrderProcessingOutcome.ConcurrentProcessing)
        {
            logger.LogInformation(
                "Order is being processed concurrently. OrderId={OrderId} MessageId={MessageId}",
                request.OrderId,
                request.MessageId);
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

    private static bool IsTransient(Exception ex) =>
        ex is TimeoutException
        || ex is IOException
        || ex is SocketException;
}

