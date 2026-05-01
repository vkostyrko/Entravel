namespace Entravel.Rmq;

public enum RabbitMqDispatchResult
{
    Ack,
    NackRequeue,
    NackDiscard
}
