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
        private readonly Type _actualReturnType;
        private readonly int _bodyArgumentIndex;
        private readonly MediaTypeCollection _contentTypes;
        private readonly RestClientContext _context;
        private readonly ITaskConverter _converter;
        private readonly Type _errorResponseType;
        private readonly HttpMethod _httpMethod = HttpMethod.Get;
        private readonly ParameterInfo[] _parameters;
        private readonly Type _returnType;
        private readonly ISerializer _serializer;
        private readonly string _template;

        public RequestHandler(MethodInfo method, RestClientContext context)
        {
            _returnType = method.ReturnType;
            _actualReturnType = GetActualReturnType();
            _parameters = method.GetParameters();
            _converter = CreateConverter(context);
            _template = context.BaseTemplate ?? "";
            _serializer = context.DefaultSerializer;
            _contentTypes = context.DefaultContentTypes;
            _errorResponseType = context.ErrorResponseType;

            _bodyArgumentIndex = _parameters.Length - 1;

            var attributes = method.GetCustomAttributes(true);
            foreach (var attribute in attributes)
                if (attribute is HttpMethodAttribute httpMethodAttribute)
                {
                    _template = _template + httpMethodAttribute.Template;

                    // TODO: support additional HTTP methods
                    if (attribute is HttpGetAttribute)
                        _httpMethod = HttpMethod.Get;
                    else if (attribute is HttpPostAttribute)
                        _httpMethod = HttpMethod.Post;
                    else if (attribute is HttpPutAttribute) _httpMethod = HttpMethod.Put;
                }
                else if (attribute is ErrorResponseAttribute errorResponseAttribute)
                {
                    _errorResponseType = errorResponseAttribute.Type;
                }
                else if (attribute is ConsumesAttribute consumesAttribute)
                {
                    _contentTypes = consumesAttribute.ContentTypes;
                    _serializer = context.SerializationManager.GetSerializer(_contentTypes);
                }
                else if (attribute is RouteAttribute routeAttribute)
                {
                    _template = _template + routeAttribute.Template;
                }

            _context = context;
        }

        private ITaskConverter CreateConverter(RestClientContext context)
        {
            return context.TaskConverterFactory.CreateTaskConverter(_actualReturnType);
        }

        private bool IsVoidTask()
        {
            return _returnType == typeof(Task);
        }

        private bool IsGenericTask()
        {
            return _returnType.BaseType == typeof(Task) && _returnType.IsGenericType;
        }

        private Type GetActualReturnType()
        {
            if (IsVoidTask())
                return typeof(void);
            if (IsGenericTask())
                return _returnType.GenericTypeArguments[0];
            return _returnType;
        }

        private string ExpandTemplate(Dictionary<string, object> routeParams)
        {
            var expanded = _template;
            foreach (var entry in routeParams)
                expanded = expanded.Replace("{" + entry.Key + "}", entry.Value.ToString());

            return expanded;
        }

        // Based on PrepareRequestMessage at https://github.com/dotnet/corefx/blob/master/src/System.Net.Http/src/System/Net/Http/HttpClient.cs
        private void PrepareRequestMessage(HttpRequestMessage request)
        {
            var baseUri = _context.BaseUri;
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
            if (_parameters.Length != args.Length)
                throw new InvalidOperationException($"Expected {_parameters.Length} parameters but got {args.Length}");

            var routeParams = GetRouteParams(args);
            var requestUri = ExpandTemplate(routeParams);
            var request = new HttpRequestMessage(_httpMethod, requestUri);

            if (_httpMethod == HttpMethod.Post || _httpMethod == HttpMethod.Put)
            {
                var mediaType = _contentTypes[0];
                request.Content = _serializer.Serialize(args[_bodyArgumentIndex], Encoding.UTF8, mediaType);
            }

            var task = SendAsync(request);

            if (IsVoidTask())
                return task;
            if (_returnType.BaseType == typeof(Task) && _returnType.IsGenericType)
                return _converter.ConvertReturnType(task);
            return task.Result;
        }

        private Dictionary<string, object> GetRouteParams(IReadOnlyList<object> args)
        {
            var routeParams = new Dictionary<string, object>();
            for (var i = 0; i < _parameters.Length; i++)
            {
                var parameterAttributes = _parameters[i].GetCustomAttributes(false);
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

            var response = await _context.HttpClient.SendAsync(request);

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
                        _context.SerializationManager.GetDeserializer(response.Content.Headers?.ContentType?.MediaType);
                    var type = response.IsSuccessStatusCode ? _actualReturnType : _errorResponseType;
                    data = await deserializer.Deserialize(response.Content, type);
                }
            }
            catch (Exception e)
            {
                var errorResponse = response.Content == null ? null : await response.Content.ReadAsStringAsync();
                throw new RestClientException(response.StatusCode, errorResponse, e);
            }

            if (response.IsSuccessStatusCode)
                return data;
            throw new RestClientException(response.StatusCode, data);
        }
    }
}