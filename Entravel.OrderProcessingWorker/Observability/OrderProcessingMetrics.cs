using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;

namespace Entravel.OrderProcessingWorker.Observability;

public static class OrderProcessingMetrics
{
    private static readonly Meter Meter = new("Entravel.OrderProcessing");
    private static readonly Counter<long> OrdersProcessedCounter = Meter.CreateCounter<long>("orders.processed");
    private static long _processedOrders;

    public static void RecordProcessedOrder(Guid orderId, string? messageId, ILogger logger)
    {
        var total = Interlocked.Increment(ref _processedOrders);
        OrdersProcessedCounter.Add(1);
        logger.LogInformation(
            "Processed order {OrderId} MessageId={MessageId}. Total processed orders: {ProcessedOrdersCount}",
            orderId,
            messageId,
            total);
    }
}

