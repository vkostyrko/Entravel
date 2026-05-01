namespace Entravel.Rmq;

public sealed class DefaultMessageRouteResolver : IMessageRouteResolver
{
    private static readonly IReadOnlyDictionary<string, string> Routes = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["OrderSubmitted"] = "orders.submitted"
    };

    public bool TryResolve(string messageType, out string routingKey) =>
        Routes.TryGetValue(messageType, out routingKey!);
}

