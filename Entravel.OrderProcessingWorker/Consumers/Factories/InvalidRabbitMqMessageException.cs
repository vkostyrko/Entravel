namespace Entravel.OrderProcessingWorker.Consumers.Factories;

public sealed class InvalidRabbitMqMessageException(string message, Exception? innerException = null)
    : Exception(message, innerException);

