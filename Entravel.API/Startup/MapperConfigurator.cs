using Entravel.API.Mapping;

namespace Entravel.API.Startup;

public static class MapperConfigurator
{
    public static void ConfigureMappings(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(OrderMappingProfile).Assembly);
    }
}
