using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using Activout.RestClient.DomainErrors;
using Microsoft.AspNetCore.Mvc;

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
            _context.DefaultSerializer = _context.SerializationManager.GetSerializer(_context.DefaultContentTypes);
        }

        private void HandleAttributes()
        {
            var attributes = _type.GetCustomAttributes();
            foreach (var attribute in attributes)
                switch (attribute)
                {
                    case DomainExceptionAttribute domainExceptionAttribute:
                        _context.DomainErrorType = domainExceptionAttribute.ErrorType;
                        _context.DomainExceptionType = domainExceptionAttribute.ExceptionType;
                        break;
                    case DomainHttpErrorAttribute domainHttpErrorAttribute:
                        _context.DomainHttpErrorAttributes.Add(domainHttpErrorAttribute);
                        break;
                    case ConsumesAttribute consumesAttribute:
                        _context.DefaultContentTypes = consumesAttribute.ContentTypes;
                        break;
                    case RouteAttribute routeAttribute:
                        _context.BaseTemplate = routeAttribute.Template;
                        break;
                    case ErrorResponseAttribute errorResponseAttribute:
                        _context.ErrorResponseType = errorResponseAttribute.Type;
                        break;
                }

            if (!_context.UseDomainException && _context.DomainHttpErrorAttributes.Any())
            {
                throw new InvalidOperationException("[DomainHttpError] requires [DomainException] on interface");
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