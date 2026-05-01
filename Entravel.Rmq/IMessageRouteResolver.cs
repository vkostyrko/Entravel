namespace Entravel.Rmq;

public interface IMessageRouteResolver
{
    bool TryResolve(string messageType, out string routingKey);
}

