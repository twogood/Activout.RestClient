using System.Collections.Concurrent;
using System.Reflection;

namespace Activout.RestClient.Implementation;

public class RestClient : DispatchProxy
{
    private readonly ConcurrentDictionary<MethodInfo, RequestHandler> _requestHandlers = new();
    private RestClientContext _context = null!;

    internal static T Create<T>(RestClientContext context) where T : class
    {
        var proxy = Create<T, RestClient>();
        ((RestClient)(object)proxy)._context = context;
        return proxy;
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        var handler = _requestHandlers.GetOrAdd(targetMethod!, m => new RequestHandler(m, _context));
        return handler.Send(args);
    }
}
