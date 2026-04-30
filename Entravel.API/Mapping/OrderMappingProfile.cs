using AutoMapper;
using Entravel.Application.Orders.SubmitOrder;
using Entravel.Contracts.Orders.SubmitOrder;

namespace Entravel.API.Mapping;

public sealed class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<OrderItemRequest, SubmitOrderItem>();
        CreateMap<SubmitOrderRequest, SubmitOrderCommand>();
        CreateMap<SubmitOrderResult, SubmitOrderResponse>();
    }
}

