using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Activout.RestClient.Helpers;
using Activout.RestClient.Serialization;
using Dynamitey.DynamicObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace Activout.RestClient.Implementation
{
    internal static class RestClientDefaults
    {
        internal static readonly MediaTypeCollection MediaTypeCollection = new MediaTypeCollection()
        {
            "application/json"
        };
    }

    internal class RestClient<T> : DynamicObject where T : class
    {
        private readonly RestClientContext _context;
        private readonly Type _type;

        private readonly Dictionary<MethodInfo, RequestHandler> _requestHandlers =
            new Dictionary<MethodInfo, RequestHandler>();

        public RestClient(RestClientContext context)
        {
            _type = typeof(T);
            _context = context;
            _context.BaseTemplate = "";
            HandleAttributes();
            if (_context.DefaultContentTypes == null)
            {
                _context.DefaultContentTypes = RestClientDefaults.MediaTypeCollection;
            }

            _context.DefaultSerializer = _context.SerializationManager.GetSerializer(_context.DefaultContentTypes);
        }

        private void HandleAttributes()
        {
            var attributes = _type.GetCustomAttributes();
            foreach (var attribute in attributes)
                if (attribute is ConsumesAttribute consumesAttribute)
                    _context.DefaultContentTypes = consumesAttribute.ContentTypes;
                else if (attribute is RouteAttribute routeAttribute)
                    _context.BaseTemplate = routeAttribute.Template;
                else if (attribute is ErrorResponseAttribute errorResponseAttribute)
                    _context.ErrorResponseType = errorResponseAttribute.Type;
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