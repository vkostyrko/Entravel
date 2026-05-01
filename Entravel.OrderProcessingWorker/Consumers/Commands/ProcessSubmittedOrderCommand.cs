using MediatR;
using Entravel.Rmq;

namespace Entravel.OrderProcessingWorker.Consumers.Commands;

public sealed record ProcessSubmittedOrderCommand(
    Guid OrderId,
    string? MessageId) : IRequest<RabbitMqDispatchResult>;

