namespace Entravel.Rmq;

public interface IRabbitMqMessageDispatcher
{
    Task<RabbitMqDispatchResult> DispatchAsync(RabbitMqMessageEnvelope envelope, CancellationToken cancellationToken);
}
