using AutoMapper;
using Entravel.Domain.Inventory;
using Entravel.EF.Infrastructure.Persistence.Entities;

namespace Entravel.EF.MappingProfile;

public sealed class InventoryEntityMappingProfile : Profile
{
    public InventoryEntityMappingProfile()
    {
        CreateMap<Inventory, InventoryEntity>()
            .ForMember(destination => destination.OrderItems, member => member.Ignore());

        CreateMap<InventoryEntity, Inventory>();
    }
}
