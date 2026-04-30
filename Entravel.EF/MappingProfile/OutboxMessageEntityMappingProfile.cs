using AutoMapper;
using Entravel.Domain.Outbox;
using Entravel.EF.Infrastructure.Persistence.Entities;

namespace Entravel.EF.MappingProfile;

public sealed class OutboxMessageEntityMappingProfile : Profile
{
    public OutboxMessageEntityMappingProfile()
    {
        CreateMap<OutboxMessage, OutboxMessageEntity>();
        CreateMap<OutboxMessageEntity, OutboxMessage>();
    }
}
