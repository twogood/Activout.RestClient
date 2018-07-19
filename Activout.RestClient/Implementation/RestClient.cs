using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Activout.RestClient.Helpers;
using Activout.RestClient.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Activout.RestClient.Implementation
{
    internal class RestClient<T> : DynamicObject where T : class
    {
        private readonly RestClientContext _context;
        private readonly Type _type;

        public RestClient(Uri baseUri, HttpClient httpClient, ISerializationManager serializationManager,
            ITaskConverterFactory taskConverterFactory)
        {
            _type = typeof(T);
            _context = new RestClientContext
            {
                BaseUri = baseUri,
                HttpClient = httpClient,
                TaskConverterFactory = taskConverterFactory,
                SerializationManager = serializationManager,
                BaseTemplate = ""
            };

            HandleAttributes();
            _context.DefaultSerializer = serializationManager.GetSerializer(_context.DefaultContentTypes);
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

            var requestHandler = new RequestHandler(method, _context);
            // TODO: cache RequestHandler

            result = requestHandler.Send(args);
            return true;
        }
    }
}