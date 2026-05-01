using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Entravel.Rmq;

public static class RabbitMqDependencyInjection
{
    public static IServiceCollection AddRabbitMqConsumers(this IServiceCollection services)
    {
        services.AddHostedService<RabbitMqConsumerHostedService>();
        return services;
    }
}
