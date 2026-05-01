using Entravel.Common;
using Entravel.Contracts.Integration.Orders;
using Entravel.Data.Repositories;
using Microsoft.Extensions.Logging;

namespace Entravel.Application.Orders.OrderSubmitted;

public sealed class OrderSubmittedMessageHandler(
    IOrderProcessingRepository orderProcessingRepository,
    ILogger<OrderSubmittedMessageHandler> logger)
{
    public async Task<OrderProcessingOutcome> HandleAsync(OrderSubmittedMessage message, CancellationToken cancellationToken)
    {
        if (message.OrderId == Guid.Empty)
        {
            logger.LogWarning("OrderSubmitted message has empty OrderId");
            return OrderProcessingOutcome.PermanentFailure;
        }

        return await orderProcessingRepository.ProcessOrderSubmittedAsync(message.OrderId, cancellationToken);
    }

    public static bool CanHandle(string messageType) =>
        string.Equals(messageType, MessageTypes.OrderSubmitted, StringComparison.Ordinal);
}
