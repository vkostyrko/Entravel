using AutoMapper;
using Entravel.Application.Orders.SubmitOrder;
using Entravel.Contracts.Orders.SubmitOrder;
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
}

