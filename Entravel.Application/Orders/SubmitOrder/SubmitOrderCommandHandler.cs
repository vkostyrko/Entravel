using MediatR;

namespace Entravel.Application.Orders.SubmitOrder;

public sealed class SubmitOrderCommandHandler : IRequestHandler<SubmitOrderCommand, SubmitOrderResult>
{
    public Task<SubmitOrderResult> Handle(SubmitOrderCommand request, CancellationToken cancellationToken)
    {
        var orderId = Guid.NewGuid();
        return Task.FromResult(new SubmitOrderResult(orderId));
    }
}

