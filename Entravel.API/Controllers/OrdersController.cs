using AutoMapper;
using Entravel.Application.Orders.GetOrderById;
using Entravel.Application.Orders.SubmitOrder;
using Entravel.Contracts.Orders.SubmitOrder;
using Entravel.Data.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Entravel.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController(IMediator mediator, IMapper mapper) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(SubmitOrderResponse), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Submit(SubmitOrderRequest request, CancellationToken cancellationToken)
    {
        var command = mapper.Map<SubmitOrderCommand>(request);
        var result = await mediator.Send(command, cancellationToken);
        return Accepted(mapper.Map<SubmitOrderResponse>(result));
    }

    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(OrderReadModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await mediator.Send(new GetOrderByIdQuery(orderId), cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }
}

