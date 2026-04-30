using AutoMapper;
using Entravel.Domain.Orders;
using Entravel.EF.Infrastructure.Persistence.Entities;

namespace Entravel.EF.MappingProfile;

public sealed class OrderEntityMappingProfile : Profile
{
    public OrderEntityMappingProfile()
    {
        CreateMap<OrderItem, OrderItemEntity>()
            .ForMember(destination => destination.Order, member => member.Ignore())
            .ForMember(destination => destination.Inventory, member => member.Ignore());

        CreateMap<Order, OrderEntity>();
    }
}
