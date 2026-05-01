using Entravel.Data.Queries;
using MediatR;

namespace Entravel.Application.Orders.GetOrderById;

public sealed record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderReadModel?>;

