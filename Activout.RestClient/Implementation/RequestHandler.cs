using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Activout.RestClient.Helpers;
using Activout.RestClient.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Activout.RestClient.Implementation
{
    internal class RequestHandler
    {
        private readonly Type actualReturnType;
        private readonly int bodyArgumentIndex;
        private readonly MediaTypeCollection contentTypes;
        private readonly RestClientContext context;
        private readonly ITaskConverter converter;
        private readonly Type errorResponseType;
        private readonly HttpMethod httpMethod = HttpMethod.Get;
        private readonly ParameterInfo[] parameters;
        private readonly Type returnType;
        private readonly ISerializer serializer;
        private readonly string template;

        public RequestHandler(MethodInfo method, RestClientContext context)
        {
            returnType = method.ReturnType;
            actualReturnType = GetActualReturnType();
            parameters = method.GetParameters();
            converter = CreateConverter(context);
            template = context.BaseTemplate ?? "";
            serializer = context.DefaultSerializer;
            contentTypes = context.DefaultContentTypes;
            errorResponseType = context.ErrorResponseType;

            bodyArgumentIndex = parameters.Length - 1;

            var attributes = method.GetCustomAttributes(true);
            foreach (var attribute in attributes)
                if (attribute is HttpMethodAttribute httpMethodAttribute)
                {
                    template = template + httpMethodAttribute.Template;

                    // TODO: support additional HTTP methods
                    if (attribute is HttpGetAttribute)
                        httpMethod = HttpMethod.Get;
                    else if (attribute is HttpPostAttribute)
                        httpMethod = HttpMethod.Post;
                    else if (attribute is HttpPutAttribute) httpMethod = HttpMethod.Put;
                }
                else if (attribute is ErrorResponseAttribute errorResponseAttribute)
                {
                    errorResponseType = errorResponseAttribute.Type;
                }
                else if (attribute is ConsumesAttribute consumesAttribute)
                {
                    contentTypes = consumesAttribute.ContentTypes;
                    serializer = context.SerializationManager.GetSerializer(contentTypes);
                }
                else if (attribute is RouteAttribute routeAttribute)
                {
                    template = template + routeAttribute.Template;
                }

            this.context = context;
        }

        private ITaskConverter CreateConverter(RestClientContext context)
        {
            return context.TaskConverterFactory.CreateTaskConverter(actualReturnType);
        }

        private bool IsVoidTask()
        {
            return returnType == typeof(Task);
        }

        private bool IsGenericTask()
        {
            return returnType.BaseType == typeof(Task) && returnType.IsGenericType;
        }

        private Type GetActualReturnType()
        {
            if (IsVoidTask())
                return typeof(void);
            if (IsGenericTask())
                return returnType.GenericTypeArguments[0];
            return returnType;
        }

        private string ExpandTemplate(Dictionary<string, object> routeParams)
        {
            var expanded = template;
            foreach (var entry in routeParams)
                expanded = expanded.Replace("{" + entry.Key + "}", entry.Value.ToString());

            return expanded;
        }

        // Based on PrepareRequestMessage at https://github.com/dotnet/corefx/blob/master/src/System.Net.Http/src/System/Net/Http/HttpClient.cs
        private void PrepareRequestMessage(HttpRequestMessage request)
        {
            var baseUri = context.BaseUri;
            Uri requestUri = null;
            if (request.RequestUri == null && baseUri == null) throw new InvalidOperationException();
            if (request.RequestUri == null)
            {
                requestUri = baseUri;
            }
            else
            {
                // If the request Uri is an absolute Uri, just use it. Otherwise try to combine it with the base Uri.
                if (!request.RequestUri.IsAbsoluteUri)
                {
                    if (baseUri == null)
                        throw new InvalidOperationException();
                    requestUri = new Uri(baseUri, request.RequestUri);
                }
            }

            // We modified the original request Uri. Assign the new Uri to the request message.
            if (requestUri != null) request.RequestUri = requestUri;

            /*
            // Add default headers
            if (_defaultRequestHeaders != null)
            {
                request.Headers.AddHeaders(_defaultRequestHeaders);
            }
            */
        }

        public object Send(object[] args)
        {
            if (parameters.Length != args.Length)
                throw new InvalidOperationException($"Expected {parameters.Length} parameters but got {args.Length}");

            var routeParams = GetRouteParams(args);
            var requestUri = ExpandTemplate(routeParams);
            var request = new HttpRequestMessage(httpMethod, requestUri);

            if (httpMethod == HttpMethod.Post || httpMethod == HttpMethod.Put)
            {
                var mediaType = contentTypes[0];
                request.Content = serializer.Serialize(args[bodyArgumentIndex], Encoding.UTF8, mediaType);
            }

            var task = SendAsync(request);

            if (IsVoidTask())
                return task;
            if (returnType.BaseType == typeof(Task) && returnType.IsGenericType)
                return converter.ConvertReturnType(task);
            return task.Result;
        }

        private Dictionary<string, object> GetRouteParams(object[] args)
        {
            var routeParams = new Dictionary<string, object>();
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameterAttributes = parameters[i].GetCustomAttributes(false);
                foreach (var attribute in parameterAttributes)
                    if (attribute.GetType() == typeof(RouteParamAttribute))
                    {
                        var routeParamAttribute = (RouteParamAttribute) attribute;
                        routeParams[routeParamAttribute.Name] = args[i];
                    }
            }

            return routeParams;
        }

        private async Task<object> SendAsync(HttpRequestMessage request)
        {
            PrepareRequestMessage(request);

            var response = await context.HttpClient.SendAsync(request);

            /*
             TODO: test cases
            if (actualReturnType == typeof(HttpResponseMessage))
            {
                return response;
            }
            else if (actualReturnType == typeof(HttpContent))
            {
                return response.Content;
            }
            */

            object data = null;
            try
            {
                if (response.Content != null)
                {
                    var deserializer =
                        context.SerializationManager.GetDeserializer(response.Content.Headers?.ContentType?.MediaType);
                    var type = response.IsSuccessStatusCode ? actualReturnType : errorResponseType;
                    data = await deserializer.Deserialize(response.Content, type);
                }
            }
            catch (Exception e)
            {
                throw new RestClientException(response.StatusCode, await response.Content.ReadAsStringAsync(), e);
            }

            if (response.IsSuccessStatusCode)
                return data;
            throw new RestClientException(response.StatusCode, data);
        }
    }
}