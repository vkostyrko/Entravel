using System.Text;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Entravel.Rmq;

public sealed class RabbitMqPublisher(IOptions<RabbitMqOptions> options, IMessageRouteResolver routeResolver) : IRabbitMqPublisher
{
    private readonly RabbitMqOptions _options = options.Value;

    public async Task PublishJsonAsync(Guid messageId, string messageType, string jsonPayload, CancellationToken cancellationToken)
    {
        if (!routeResolver.TryResolve(messageType, out var routingKey))
        {
            throw new InvalidOperationException($"Unknown message type: {messageType}");
        }

        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.Username,
            Password = _options.Password
        };

        await using var connection = await factory.CreateConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync();

        var exchangeName = _options.ResolvedExchangeName;

        await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        var props = new BasicProperties
        {
            MessageId = messageId.ToString(),
            ContentType = "application/json",
            Type = messageType,
            DeliveryMode = DeliveryModes.Persistent
        };

        var body = Encoding.UTF8.GetBytes(jsonPayload);

        await channel.BasicPublishAsync(
            exchange: exchangeName,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: props,
            body: body,
            cancellationToken: cancellationToken);
    }
}

