using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using Activout.RestClient.DomainExceptions;

namespace Activout.RestClient.Implementation
{
    internal class RestClient<T> : DynamicObject where T : class
    {
        private readonly RestClientContext _context;
        private readonly Type _type;

        private readonly IDictionary<MethodInfo, RequestHandler> _requestHandlers =
            new ConcurrentDictionary<MethodInfo, RequestHandler>();

        public RestClient(RestClientContext context)
        {
            _type = typeof(T);
            _context = context;
            HandleAttributes();
            _context.DefaultSerializer = _context.SerializationManager.GetSerializer(_context.DefaultContentType);
        }

        private void HandleAttributes()
        {
            var attributes = _type.GetCustomAttributes();
            foreach (var attribute in attributes)
                switch (attribute)
                {
                    case ContentTypeAttribute contentTypeAttribute:
                        _context.DefaultContentType = MediaType.ValueOf(contentTypeAttribute.ContentType);
                        break;
                    case DomainExceptionAttribute domainExceptionAttribute:
                        _context.DomainExceptionType = domainExceptionAttribute.Type;
                        break;
                    case ErrorResponseAttribute errorResponseAttribute:
                        _context.ErrorResponseType = errorResponseAttribute.Type;
                        break;
                    case HeaderAttribute headerAttribute:
                        _context.DefaultHeaders.AddOrReplaceHeader(headerAttribute.Name, headerAttribute.Value, headerAttribute.Replace);
                        break;
                    case PathAttribute pathAttribute:
                        _context.BaseTemplate = pathAttribute.Template;
                        break;
                }
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _type.GetMembers().Select(x => x.Name);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
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

            result = requestHandler.Send(args);
            return true;
        }
    }
}