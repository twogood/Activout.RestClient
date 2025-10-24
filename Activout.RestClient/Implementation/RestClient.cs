using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Activout.RestClient.Implementation
{
    internal class RestClient : DynamicObject
    {
        private readonly RestClientContext _context;
        private readonly Type _type;
        private readonly ConcurrentDictionary<MethodInfo, RequestHandler> _requestHandlers = new();
        //private readonly ISerializer? _defaultSerializer;

        public RestClient(Type type, RestClientContext context)
        {
            _type = type;
            _context = context;
            //_defaultSerializer = _context.SerializationManager.GetSerializer(_context.DefaultContentType);
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _type.GetMembers().Select(x => x.Name);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result)
        {
            var method = _type.GetMethod(binder.Name);
            if (method == null)
            {
                result = null;
                return false;
            }

            if (!_requestHandlers.TryGetValue(method, out var requestHandler))
            {
                requestHandler = new RequestHandler(method, _context);
                _requestHandlers[method] = requestHandler;
            }

            result = requestHandler.Send(args ?? []);
            return true;
        }
    }
}