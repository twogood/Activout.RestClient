using Activout.RestClient.Helpers;
using Activout.RestClient.Serialization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Reflection;

namespace Activout.RestClient.Implementation
{
    class RestClient<T> : DynamicObject where T : class
    {
        private readonly Type type;
        private readonly RestClientContext context;

        public RestClient(Uri baseUri, HttpClient httpClient, ISerializationManager serializationManager, ITaskConverterFactory taskConverterFactory)
        {
            type = typeof(T);
            context = new RestClientContext
            {
                BaseUri = baseUri,
                HttpClient = httpClient,
                TaskConverterFactory = taskConverterFactory,
                SerializationManager = serializationManager,
                BaseTemplate = "",
            };

            HandleAttributes();
            context.DefaultSerializer = serializationManager.GetSerializer(context.DefaultContentTypes);
        }

        private void HandleAttributes()
        {
            var attributes = type.GetCustomAttributes();
            foreach (var attribute in attributes)
            {
                if (attribute is ConsumesAttribute consumesAttribute)
                {
                    context.DefaultContentTypes = consumesAttribute.ContentTypes;
                }
                else if (attribute is InterfaceConsumesAttribute interfaceConsumesAttribute)
                {
                    context.DefaultContentTypes = interfaceConsumesAttribute.ContentTypes;
                }
                else if (attribute is RouteAttribute routeAttribute)
                {
                    context.BaseTemplate = routeAttribute.Template;
                }
                else if (attribute is InterfaceRouteAttribute interfaceRouteAttribute)
                {
                    context.BaseTemplate = interfaceRouteAttribute.Template;
                }
                else if (attribute is ErrorResponseAttribute errorResponseAttribute)
                {
                    context.ErrorResponseType = errorResponseAttribute.Type;
                }
            }
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return type.GetMembers().Select(x => x.Name);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var method = type.GetMethod(binder.Name);
            if (method == null)
            {
                result = null;
                return false;
            }

            var requestHandler = new RequestHandler(method, context);
            // TODO: cache RequestHandler

            result = requestHandler.Send(args);
            return true;
        }
    }
}
