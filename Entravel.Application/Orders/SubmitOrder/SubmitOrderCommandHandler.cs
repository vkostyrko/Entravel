using System.Text.Json;
using Entravel.Common;
using Entravel.Data.Repositories;
using Entravel.Domain.Orders;
using Entravel.Domain.Outbox;
using MediatR;

namespace Entravel.Application.Orders.SubmitOrder;

public sealed class SubmitOrderCommandHandler(
    IOrderRepository orderRepository,
    IOutboxMessageRepository outboxMessageRepository)
    : IRequestHandler<SubmitOrderCommand, SubmitOrderResult>
{
    public async Task<SubmitOrderResult> Handle(SubmitOrderCommand command, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var orderId = Guid.NewGuid();

        var order = CreateOrderWithItems(command, orderId, now);
        var outboxMessage = CreateOutboxMessage(orderId, now);

        await orderRepository.AddAsync(order, cancellationToken);
        await outboxMessageRepository.AddAsync(outboxMessage, cancellationToken);

        return new SubmitOrderResult(orderId);
    }

    private static Order CreateOrderWithItems(SubmitOrderCommand command, Guid orderId, DateTime now)
    {
        var order = new Order
        {
            Id = orderId,
            CustomerId = command.CustomerId,
            TotalAmount = command.TotalAmount,
            Status = OrderStatus.Pending,
            CreatedDate = now
        };

        foreach (var requestItem in command.Items)
        {
            order.Items.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                InventoryId = requestItem.InventoryId,
                Quantity = requestItem.Quantity,
                CreatedDate = now
            });
        }

        return order;
    }

    private static OutboxMessage CreateOutboxMessage(Guid orderId, DateTime now) =>
        new()
        {
            Id = Guid.NewGuid(),
            Type = MessageTypes.OrderSubmitted,
            Payload = JsonSerializer.Serialize(new { OrderId = orderId }),
            Status = OutboxMessageStatus.New,
            CreatedDate = now
        };
}
