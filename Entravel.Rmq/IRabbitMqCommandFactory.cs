namespace Entravel.Rmq;

public interface IRabbitMqCommandFactory
{
    string MessageType { get; }

    object Create(ReadOnlyMemory<byte> body, RabbitMqMessageEnvelope envelope);
}

