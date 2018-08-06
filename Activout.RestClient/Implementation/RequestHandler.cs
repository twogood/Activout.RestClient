using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Activout.RestClient.Attributes;
using Activout.RestClient.Helpers;
using Activout.RestClient.ParamConverter;
using Activout.RestClient.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Caching.Memory;

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
        private readonly IParamConverter[] _paramConverters;
        private CacheResponseAttribute _cacheResponse;

        public RequestHandler(MethodInfo method, RestClientContext context)
        {
            _context = context;
            _returnType = method.ReturnType;
            _actualReturnType = GetActualReturnType();
            _parameters = method.GetParameters();
            _paramConverters = GetParamConverters(context.ParamConverterManager);
            _converter = CreateConverter(context);
            _template = context.BaseTemplate ?? "";
            _serializer = context.DefaultSerializer;
            _contentTypes = context.DefaultContentTypes;
            _errorResponseType = context.ErrorResponseType;

            _bodyArgumentIndex = _parameters.Length - 1;

            var templateBuilder = new StringBuilder(context.BaseTemplate ?? "");
            foreach (var attribute in method.GetCustomAttributes(true))
                switch (attribute)
                {
                    case HttpMethodAttribute httpMethodAttribute:
                        templateBuilder.Append(httpMethodAttribute.Template);
                        _httpMethod = GetHttpMethod(httpMethodAttribute);
                        break;

                    case ErrorResponseAttribute errorResponseAttribute:
                        _errorResponseType = errorResponseAttribute.Type;
                        break;

                    case ConsumesAttribute consumesAttribute:
                        _contentTypes = consumesAttribute.ContentTypes;
                        _serializer = context.SerializationManager.GetSerializer(_contentTypes);
                        break;

                    case RouteAttribute routeAttribute:
                        templateBuilder.Append(routeAttribute.Template);
                        break;

                    case CacheResponseAttribute cacheResponseAttribute:
                        Preconditions.CheckNotNull(context.ResponseCache,
                            "IRequestBuilder.With(responseCache) is required to support [CacheResponse]");
                        _cacheResponse = cacheResponseAttribute;
                        break;
                }

            _template = templateBuilder.ToString();
        }

        private IParamConverter[] GetParamConverters(IParamConverterManager paramConverterManager)
        {
            var paramConverters = new IParamConverter[_parameters.Length];
            for (var i = 0; i < _parameters.Length; i++)
            {
                paramConverters[i] = paramConverterManager.GetConverter(_parameters[i]);
            }

            return paramConverters;
        }

        private static HttpMethod GetHttpMethod(HttpMethodAttribute attribute)
        {
            switch (attribute)
            {
                case HttpGetAttribute _:
                    return HttpMethod.Get;
                case HttpPostAttribute _:
                    return HttpMethod.Post;
                case HttpPutAttribute _:
                    return HttpMethod.Put;
                default:
                    throw new NotImplementedException($"Http Attribute not yet supported: {attribute}");
            }
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
        }

        public object Send(object[] args)
        {
            if (_parameters.Length != args.Length)
                throw new InvalidOperationException($"Expected {_parameters.Length} parameters but got {args.Length}");

            var (routeParams, queryParams, formParams, headerParams) = GetParams(args);
            var requestUriString = ExpandTemplate(routeParams);
            if (queryParams.Any())
            {
                requestUriString = requestUriString + "?" + string.Join("&", queryParams);
            }

            var requestUri = new Uri(requestUriString, UriKind.RelativeOrAbsolute);

            var request = new HttpRequestMessage(_httpMethod, requestUri);

            SetHeaders(request, headerParams);

            if (_httpMethod == HttpMethod.Post || _httpMethod == HttpMethod.Put)
            {
                if (formParams.Any())
                {
                    request.Content = new FormUrlEncodedContent(formParams);
                }
                else
                {
                    var mediaType = _contentTypes[0];
                    request.Content = _serializer.Serialize(args[_bodyArgumentIndex], Encoding.UTF8, mediaType);
                }
            }

            var task = SendAsync(request);

            if (IsVoidTask())
                return task;
            if (_returnType.BaseType == typeof(Task) && _returnType.IsGenericType)
                return _converter.ConvertReturnType(task);
            return task.Result;
        }

        private void SetHeaders(HttpRequestMessage request, List<KeyValuePair<string, string>> headerParams)
        {
            _context.DefaultHeaders.ForEach(p => request.Headers.Add(p.Key, p.Value.ToString()));
            headerParams.ForEach(p => request.Headers.Add(p.Key, p.Value));
        }

        private (Dictionary<string, object>, List<string>, List<KeyValuePair<string, string>>,
            List<KeyValuePair<string, string>>) GetParams(
                IReadOnlyList<object> args)
        {
            var routeParams = new Dictionary<string, object>();
            var queryParams = new List<string>();
            var formParams = new List<KeyValuePair<string, string>>();
            var headerParams = new List<KeyValuePair<string, string>>();

            for (var i = 0; i < _parameters.Length; i++)
            {
                var parameterAttributes = _parameters[i].GetCustomAttributes(false);
                var name = _parameters[i].Name;
                var value = _paramConverters[i].ToString(args[i]);
                var escapedValue = Uri.EscapeDataString(value);
                var handled = false;

                foreach (var attribute in parameterAttributes)
                {
                    if (attribute is RouteParamAttribute routeParamAttribute)
                    {
                        routeParams[routeParamAttribute.Name ?? name] = escapedValue;
                        handled = true;
                    }
                    else if (attribute is QueryParamAttribute queryParamAttribute)
                    {
                        name = queryParamAttribute.Name ?? name;
                        queryParams.Add(Uri.EscapeDataString(name) + "=" + escapedValue);
                        handled = true;
                    }
                    else if (attribute is FormParamAttribute formParamAttribute)
                    {
                        name = formParamAttribute.Name ?? name;
                        formParams.Add(new KeyValuePair<string, string>(name, value));
                        handled = true;
                    }
                    else if (attribute is HeaderParamAttribute headerParamAttribute)
                    {
                        name = headerParamAttribute.Name ?? name;
                        headerParams.Add(new KeyValuePair<string, string>(name, value));
                        handled = true;
                    }
                }

                if (!handled)
                {
                    routeParams[name] = escapedValue;
                }
            }

            return (routeParams, queryParams, formParams, headerParams);
        }

        private async Task<object> SendAsync(HttpRequestMessage request)
        {
            PrepareRequestMessage(request);

            object cacheKey = null;
            if (_cacheResponse != null)
            {
                cacheKey = _context.ResponseCache.CreateKey(request);
                if (_context.ResponseCache.TryGetValue(cacheKey, out var result))
                {
                    return result;
                }
            }

            var response = await _context.HttpClient.SendAsync(request);

            var type = response.IsSuccessStatusCode ? _actualReturnType : _errorResponseType;
            object data;

            try
            {
                if (type == typeof(HttpResponseMessage))
                {
                    data = response;
                }
                else if (type == typeof(HttpContent))
                {
                    data = response.Content;
                }
                else if (type == typeof(void) || response.Content == null)
                {
                    data = null;
                }
                else
                {
                    var contentTypeMediaType = response.Content.Headers?.ContentType?.MediaType;
                    var deserializer =
                        _context.SerializationManager.GetDeserializer(contentTypeMediaType);
                    if (deserializer == null)
                    {
                        throw new RestClientException(response.StatusCode,
                            "No deserializer found for " + contentTypeMediaType);
                    }

                    data = await deserializer.Deserialize(response.Content, type);
                }
            }
            catch (Exception e)
            {
                var errorResponse = response.Content == null ? null : await response.Content.ReadAsStringAsync();
                throw new RestClientException(response.StatusCode, errorResponse, e);
            }

            if (response.IsSuccessStatusCode)
            {
                if (cacheKey != null)
                {
                    _context.ResponseCache.TrySetValue(cacheKey, data, response);
                }

                return data;
            }

            throw new RestClientException(response.StatusCode, data);
        }

    }
}