namespace Entravel.Rmq;

public interface IRabbitMqPublisher
{
    Task PublishJsonAsync(
        Guid messageId,
        string messageType,
        string jsonPayload,
        CancellationToken cancellationToken);
}

