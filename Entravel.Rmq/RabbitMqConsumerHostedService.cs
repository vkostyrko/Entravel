using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Entravel.Rmq;

public sealed class RabbitMqConsumerHostedService(
    IOptions<RabbitMqOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger<RabbitMqConsumerHostedService> logger)
    : IHostedService
{
    private CancellationTokenSource? _linkedCts;
    private Task[]? _consumerTasks;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var rabbitOptions = options.Value;
        if (rabbitOptions.Subscriptions.Count == 0)
        {
            logger.LogWarning("RabbitMQ consumer not started: no subscriptions configured.");
            return Task.CompletedTask;
        }

        _linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var token = _linkedCts.Token;

        _consumerTasks = rabbitOptions.Subscriptions
            .Select(subscription => Task.Run(() => RunSubscriptionAsync(rabbitOptions, subscription, token), CancellationToken.None))
            .ToArray();

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_linkedCts is not null)
        {
            await _linkedCts.CancelAsync();
        }

        if (_consumerTasks is { Length: > 0 })
        {
            try
            {
                await Task.WhenAll(_consumerTasks).WaitAsync(TimeSpan.FromSeconds(30), cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Consumer tasks did not complete cleanly during shutdown.");
            }
        }
    }

    private async Task RunSubscriptionAsync(
        RabbitMqOptions rabbitOptions,
        RabbitMqSubscriptionOptions subscription,
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            IConnection? connection = null;
            IChannel? channel = null;
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = rabbitOptions.Host,
                    Port = rabbitOptions.Port,
                    UserName = rabbitOptions.Username,
                    Password = rabbitOptions.Password
                };

                connection = await factory.CreateConnectionAsync(stoppingToken);
                channel = await connection.CreateChannelAsync();

                var prefetch = (ushort)Math.Clamp(rabbitOptions.PrefetchCount, 1, ushort.MaxValue);
                await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: prefetch, global: false, cancellationToken: stoppingToken);

                var exchangeType = ResolveExchangeType(subscription.ExchangeType);

                await channel.ExchangeDeclareAsync(
                    exchange: subscription.ExchangeName,
                    type: exchangeType,
                    durable: true,
                    autoDelete: false,
                    arguments: null,
                    cancellationToken: stoppingToken);

                await channel.QueueDeclareAsync(
                    queue: subscription.QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null,
                    cancellationToken: stoppingToken);

                await channel.QueueBindAsync(
                    queue: subscription.QueueName,
                    exchange: subscription.ExchangeName,
                    routingKey: subscription.RoutingKey,
                    arguments: null,
                    cancellationToken: stoppingToken);

                var ackLock = new SemaphoreSlim(1, 1);
                var consumer = new AsyncEventingBasicConsumer(channel);

                consumer.ReceivedAsync += async (_, ea) =>
                {
                    try
                    {
                        var envelope = new RabbitMqMessageEnvelope
                        {
                            ConfiguredMessageType = subscription.MessageType,
                            PropertyMessageType = ea.BasicProperties?.Type,
                            RoutingKey = ea.RoutingKey ?? string.Empty,
                            RabbitMqMessageId = ea.BasicProperties?.MessageId,
                            Body = ea.Body
                        };

                        RabbitMqDispatchResult result;
                        await using (var scope = scopeFactory.CreateAsyncScope())
                        {
                            var dispatcher = scope.ServiceProvider.GetRequiredService<IRabbitMqMessageDispatcher>();
                            result = await dispatcher.DispatchAsync(envelope, stoppingToken);
                        }

                        await ackLock.WaitAsync(stoppingToken);
                        try
                        {
                            switch (result)
                            {
                                case RabbitMqDispatchResult.Ack:
                                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                                    break;
                                case RabbitMqDispatchResult.NackRequeue:
                                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
                                    break;
                                default:
                                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
                                    break;
                            }
                        }
                        finally
                        {
                            ackLock.Release();
                        }
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Unhandled error while processing RabbitMQ message on queue {QueueName}", subscription.QueueName);
                        await ackLock.WaitAsync(stoppingToken);
                        try
                        {
                            await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
                        }
                        finally
                        {
                            ackLock.Release();
                        }
                    }
                };

                await channel.BasicConsumeAsync(
                    queue: subscription.QueueName,
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: stoppingToken);

                logger.LogInformation(
                    "RabbitMQ consumer started: queue={Queue}, exchange={Exchange}, routingKey={RoutingKey}, messageType={MessageType}",
                    subscription.QueueName,
                    subscription.ExchangeName,
                    subscription.RoutingKey,
                    subscription.MessageType);

                try
                {
                    await Task.Delay(Timeout.Infinite, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "RabbitMQ consumer loop error for queue {QueueName}; retrying in 5s", subscription.QueueName);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            finally
            {
                if (channel is not null)
                {
                    try
                    {
                        await channel.CloseAsync(cancellationToken: CancellationToken.None);
                    }
                    catch
                    {
                    }

                    await channel.DisposeAsync();
                }

                if (connection is not null)
                {
                    try
                    {
                        await connection.CloseAsync(cancellationToken: CancellationToken.None);
                    }
                    catch
                    {
                    }

                    await connection.DisposeAsync();
                }
            }
        }
    }

    private static string ResolveExchangeType(string? exchangeType) =>
        exchangeType?.Trim().ToLowerInvariant() switch
        {
            "direct" => ExchangeType.Direct,
            "fanout" => ExchangeType.Fanout,
            "headers" => ExchangeType.Headers,
            _ => ExchangeType.Topic
        };
}
