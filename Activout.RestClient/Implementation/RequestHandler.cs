using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Activout.RestClient.DomainExceptions;
using Activout.RestClient.Helpers;
using Activout.RestClient.ParamConverter;
using Activout.RestClient.Serialization;
using Microsoft.Extensions.Logging;

namespace Activout.RestClient.Implementation;

internal class RequestHandler
{
    // https://www.w3.org/Protocols/rfc2616/rfc2616-sec7.html#sec7.2.1
    private const string DefaultHttpContentType = "application/octet-stream";

    // https://tools.ietf.org/html/rfc7578#section-4.4
    private static readonly MediaType DefaultPartContentType = new MediaType("text/plain");

    private readonly Type _actualReturnType;
    private readonly int _bodyArgumentIndex = -1;
    private readonly MediaType _contentType;
    private readonly RestClientContext _context;
    private readonly ITaskConverter? _converter;
    private readonly Type _errorResponseType;
    private readonly HttpMethod _httpMethod = HttpMethod.Get;
    private readonly ParameterInfo[] _parameters;
    private readonly Type _returnType;
    private readonly ISerializer _serializer;
    private readonly string _template;
    private readonly IParamConverter[] _paramConverters;
    private readonly IDomainExceptionMapper? _domainExceptionMapper;
    private readonly List<KeyValuePair<string, object>> _requestHeaders = new List<KeyValuePair<string, object>>();

    private bool IsDebugLoggingEnabled => _context.Logger.IsEnabled(LogLevel.Debug);

    public RequestHandler(MethodInfo method, RestClientContext context)
    {
        _returnType = method.ReturnType;
        _actualReturnType = GetActualReturnType();
        _parameters = method.GetParameters();
        _paramConverters = GetParamConverters(context.ParamConverterManager);
        _converter = CreateConverter(context);
        _template = context.BaseTemplate;
        _serializer = context.DefaultSerializer;
        _contentType = context.DefaultContentType;
        _errorResponseType = context.ErrorResponseType ?? typeof(string);
        _requestHeaders.AddRange(context.DefaultHeaders);

        var templateBuilder = new StringBuilder(context.BaseTemplate);
        foreach (var attribute in method.GetCustomAttributes(true))
            switch (attribute)
            {
                case ContentTypeAttribute contentTypeAttribute:
                    _contentType = new MediaType(contentTypeAttribute.ContentType);
                    break;

                case ErrorResponseAttribute errorResponseAttribute:
                    _errorResponseType = errorResponseAttribute.Type;
                    break;

                case HeaderAttribute headerAttribute:
                    _requestHeaders.AddOrReplaceHeader(headerAttribute.Name, headerAttribute.Value,
                        headerAttribute.Replace);
                    break;

                case HttpMethodAttribute httpMethodAttribute:
                    templateBuilder.Append(httpMethodAttribute.Template);
                    _httpMethod = GetHttpMethod(httpMethodAttribute);
                    break;

                case PathAttribute pathAttribute:
                    templateBuilder.Append(pathAttribute.Template);
                    break;
            }

        if (IsHttpMethodWithBody())
        {
            _bodyArgumentIndex = _parameters.Length - 1;

            if (_parameters.Length > 0 &&
                _parameters[_bodyArgumentIndex].ParameterType == typeof(CancellationToken))
            {
                _bodyArgumentIndex--;
            }

            if (_bodyArgumentIndex < 0)
            {
                throw new InvalidOperationException("No body argument found for method: " + method.Name);
            }
        }

        _serializer = context.SerializationManager.GetSerializer(_contentType) ??
                      throw new InvalidOperationException("No serializer found for content type: " + _contentType);

        if (context.DomainExceptionType != null)
        {
            _domainExceptionMapper = context.DomainExceptionMapperFactory.CreateDomainExceptionMapper(
                method,
                _errorResponseType,
                context.DomainExceptionType);
        }

        _template = templateBuilder.ToString();
        _context = context;
    }

    private bool IsHttpMethodWithBody()
    {
        return _httpMethod == HttpMethod.Post || _httpMethod == HttpMethod.Put || _httpMethod == HttpMethod.Patch;
    }

    private IParamConverter[] GetParamConverters(IParamConverterManager paramConverterManager)
    {
        var paramConverters = new IParamConverter[_parameters.Length];
        for (var i = 0; i < _parameters.Length; i++)
        {
            paramConverters[i] = paramConverterManager.GetConverter(_parameters[i].ParameterType, _parameters[i])
                                 ?? throw new InvalidOperationException(
                                     "No parameter converter found for type: " + _parameters[i].ParameterType);
        }

        return paramConverters;
    }

    private static HttpMethod GetHttpMethod(HttpMethodAttribute attribute)
    {
        return attribute.HttpMethod;
    }

    private ITaskConverter? CreateConverter(RestClientContext context)
    {
        if (_actualReturnType == typeof(void))
        {
            return null;
        }

        return context.TaskConverterFactory.CreateTaskConverter(_actualReturnType) ??
               throw new InvalidOperationException("Failed to create task converter for return type: " +
                                                   _actualReturnType);
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
        Uri? requestUri = null;
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

    public object? Send(object?[]? args)
    {
        var headers = new List<KeyValuePair<string, object>>();
        headers.AddRange(_requestHeaders);

        var routeParams = new Dictionary<string, object>();
        var queryParams = new List<string>();
        var formParams = new List<KeyValuePair<string, string>>();
        var partParams = new List<Part<HttpContent>>();
        var cancellationToken = GetParams(args, routeParams, queryParams, formParams, headers, partParams);

        var requestUriString = ExpandTemplate(routeParams);
        if (queryParams.Count != 0)
        {
            requestUriString = requestUriString + "?" + string.Join("&", queryParams);
        }

        var requestUri = new Uri(requestUriString, UriKind.RelativeOrAbsolute);

        var request = new HttpRequestMessage(_httpMethod, requestUri);

        SetHeaders(request, headers);

        if (IsHttpMethodWithBody())
        {
            if (partParams.Count != 0)
            {
                request.Content = CreateMultipartFormDataContent(partParams);
            }
            else if (formParams.Count != 0)
            {
                request.Content = new FormUrlEncodedContent(formParams);
            }
            else if (args != null)
            {
                request.Content = GetHttpContent(_serializer, args[_bodyArgumentIndex], _contentType);
            }
        }

        var task = SendRequestAndHandleResponse(request, cancellationToken);

        if (IsVoidTask())
            return task;
        if (_returnType.BaseType == typeof(Task) && _returnType.IsGenericType && _converter != null)
            return _converter.ConvertReturnType(task);
        return task.Result;
    }

    private static MultipartFormDataContent CreateMultipartFormDataContent(
        IEnumerable<Part<HttpContent>> partParams)
    {
        var content = new MultipartFormDataContent();
        foreach (var part in partParams)
        {
            if (!string.IsNullOrEmpty(part.FileName))
            {
                content.Add(part.Content, part.Name, part.FileName);
            }
            else if (!string.IsNullOrEmpty(part.Name))
            {
                content.Add(part.Content, part.Name);
            }
            else
            {
                content.Add(part.Content);
            }
        }

        return content;
    }

    private void SetHeaders(HttpRequestMessage request, List<KeyValuePair<string, object>> headers)
    {
        headers.ForEach(p => request.Headers.Add(p.Key, p.Value.ToString()));
    }

    private string? ConvertValueToString(object? value, ParameterInfo parameterInfo)
    {
        if (value == null)
            return null;

        var converter = _context.ParamConverterManager.GetConverter(value.GetType(), parameterInfo);
        return converter?.ToString(value) ?? value.ToString();
    }

    private CancellationToken GetParams(
        object?[]? args,
        Dictionary<string, object> pathParams,
        List<string> queryParams,
        List<KeyValuePair<string, string>> formParams,
        List<KeyValuePair<string, object>> headers,
        List<Part<HttpContent>> parts)
    {
        var cancellationToken = CancellationToken.None;

        if (_parameters.Length > 0 && args == null || _parameters.Length != args?.Length)
        {
            throw new InvalidOperationException(
                $"Argument count mismatch. Expected: {_parameters.Length}, Actual: {args?.Length ?? 0}");
        }

        for (var i = 0; i < _parameters.Length; i++)
        {
            var rawValue = args[i];
            if (rawValue is CancellationToken ct)
            {
                cancellationToken = ct;
                continue;
            }

            if (rawValue == null)
            {
                continue;
            }

            var parameterAttributes = _parameters[i].GetCustomAttributes(false);
            var parameterName = _parameters[i].Name ?? throw new InvalidOperationException(
                "Parameter name not found for parameter at index: " + i);
            var handled = false;

            foreach (var attribute in parameterAttributes)
            {
                if (attribute is PartParamAttribute partAttribute)
                {
                    if (_parameters[i].ParameterType.IsArray)
                    {
                        if (rawValue is IEnumerable items)
                        {
                            foreach (var item in items)
                            {
                                parts.AddRange(GetPartNameAndHttpContent(partAttribute, parameterName, item));
                            }
                        }
                    }
                    else
                    {
                        parts.AddRange(GetPartNameAndHttpContent(partAttribute, parameterName, rawValue));
                    }

                    handled = true;
                }
                else if (attribute is PathParamAttribute pathParamAttribute)
                {
                    var stringValue = _paramConverters[i].ToString(rawValue);
                    pathParams[pathParamAttribute.Name ?? parameterName] = Uri.EscapeDataString(stringValue);
                    handled = true;
                }
                else if (attribute is QueryParamAttribute queryParamAttribute)
                {
                    if (rawValue is IDictionary dictionary)
                    {
                        foreach (DictionaryEntry entry in dictionary)
                        {
                            var key = entry.Key.ToString();
                            var value = ConvertValueToString(entry.Value, _parameters[i]);
                            if (key != null && value != null)
                            {
                                queryParams.Add(Uri.EscapeDataString(key) + "=" + Uri.EscapeDataString(value));
                            }
                        }
                    }
                    else if (rawValue != null)
                    {
                        var stringValue = _paramConverters[i].ToString(rawValue);
                        queryParams.Add(Uri.EscapeDataString(queryParamAttribute.Name ?? parameterName) + "=" +
                                        Uri.EscapeDataString(stringValue));
                    }

                    handled = true;
                }
                else if (attribute is FormParamAttribute formParamAttribute)
                {
                    if (rawValue is IDictionary dictionary)
                    {
                        foreach (DictionaryEntry entry in dictionary)
                        {
                            var key = entry.Key.ToString();
                            var value = ConvertValueToString(entry.Value, _parameters[i]);
                            if (key != null && value != null)
                            {
                                formParams.Add(new KeyValuePair<string, string>(key, value));
                            }
                        }
                    }
                    else if (rawValue != null)
                    {
                        var stringValue = _paramConverters[i].ToString(rawValue);
                        formParams.Add(new KeyValuePair<string, string>(formParamAttribute.Name ?? parameterName,
                            stringValue));
                    }

                    handled = true;
                }
                else if (attribute is HeaderParamAttribute headerParamAttribute)
                {
                    if (rawValue is IDictionary dictionary)
                    {
                        foreach (DictionaryEntry entry in dictionary)
                        {
                            var key = entry.Key.ToString();
                            var value = ConvertValueToString(entry.Value, _parameters[i]);
                            if (key != null && value != null)
                            {
                                headers.AddOrReplaceHeader(key, value, headerParamAttribute.Replace);
                            }
                        }
                    }
                    else if (rawValue != null)
                    {
                        var stringValue = _paramConverters[i].ToString(rawValue);
                        headers.AddOrReplaceHeader(headerParamAttribute.Name ?? parameterName, stringValue,
                            headerParamAttribute.Replace);
                    }

                    handled = true;
                }
            }

            if (!handled)
            {
                var stringValue = _paramConverters[i].ToString(rawValue);
                pathParams[parameterName] = Uri.EscapeDataString(stringValue);
            }
        }

        return cancellationToken;
    }

    private IEnumerable<Part<HttpContent>> GetPartNameAndHttpContent(PartParamAttribute partAttribute,
        string parameterName,
        object? rawValue)
    {
        string? fileName = null;
        string? partName = null;

        if (rawValue is Part part)
        {
            rawValue = part.InternalContent;
            partName = part.Name;
            fileName = part.FileName;
        }

        if (rawValue is { })
        {
            yield return new Part<HttpContent>
            {
                Content = GetPartHttpContent(partAttribute, rawValue),
                Name = partName ?? partAttribute.Name ?? parameterName,
                FileName = fileName ?? partAttribute.FileName
            };
        }
    }

    private HttpContent GetPartHttpContent(PartParamAttribute partAttribute, object value)
    {
        // TODO: prepare part serializer in advance

        var contentType = partAttribute.ContentType ?? DefaultPartContentType;
        var serializer = _context.SerializationManager.GetSerializer(contentType) ??
                         throw new InvalidOperationException("No serializer for part content type: " + contentType);
        return GetHttpContent(serializer, value, contentType);
    }

    private static HttpContent GetHttpContent(ISerializer serializer, object? value, MediaType contentType)
    {
        if (value is HttpContent httpContent)
        {
            return httpContent;
        }

        if (serializer == null)
        {
            throw new InvalidOperationException("No serializer for: " + contentType);
        }

        return serializer.Serialize(value, Encoding.UTF8, contentType);
    }


    private async Task<object?> SendRequestAndHandleResponse(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await SendRequest(request, cancellationToken);
        return await HandleResponse(request, response);
    }

    private async Task<HttpResponseMessage> SendRequest(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        PrepareRequestMessage(request);

        if (IsDebugLoggingEnabled)
        {
            _context.Logger.LogDebug("{Request}", request);

            if (request.Content != null)
            {
                await request.Content.LoadIntoBufferAsync();
                _context.Logger.LogDebug("{RequestContent}",
                    (await request.Content.ReadAsStringAsync(cancellationToken)).SafeSubstring(0, 1000));
            }
        }

        HttpResponseMessage response;
        using (_context.RequestLogger.TimeOperation(request))
        {
            response = await _context.HttpClient.SendAsync(request, cancellationToken);
        }

        if (IsDebugLoggingEnabled)
        {
            _context.Logger.LogDebug("{Response}", response);

            await response.Content.LoadIntoBufferAsync();
            _context.Logger.LogDebug("{ResponseContent}",
                (await response.Content.ReadAsStringAsync(cancellationToken)).SafeSubstring(0, 1000));
        }

        return response;
    }

    private async Task<object?> HandleResponse(HttpRequestMessage request, HttpResponseMessage response)
    {
        if (_actualReturnType == typeof(HttpResponseMessage))
        {
            return response;
        }

        var shouldDisposeResponse = true;
        try
        {
            if (_actualReturnType == typeof(HttpStatusCode))
            {
                return response.StatusCode;
            }

            object? data;
            var type = response.IsSuccessStatusCode ? _actualReturnType : _errorResponseType;

            if (type == typeof(void))
            {
                data = null;
            }
            else if (type.IsInstanceOfType(response.Content)) // HttpContent or a subclass like MultipartFormDataContent
            {
                shouldDisposeResponse = false;
                data = response.Content;
            }
            else
            {
                data = await Deserialize(request, response, type);
            }

            if (response.IsSuccessStatusCode)
            {
                return data;
            }

            if (_context.UseDomainException && _domainExceptionMapper != null)
            {
                throw await _domainExceptionMapper.CreateExceptionAsync(response, data);
            }

            throw new RestClientException(request.RequestUri, response.StatusCode, data);
        }
        finally
        {
            if (shouldDisposeResponse)
            {
                response.Dispose();
            }
        }
    }

    private async Task<object?> Deserialize(HttpRequestMessage request, HttpResponseMessage response, Type type)
    {
        var contentTypeMediaType = response.Content.Headers.ContentType?.MediaType ?? DefaultHttpContentType;
        var deserializer = _context.SerializationManager.GetDeserializer(new MediaType(contentTypeMediaType)) ??
                           throw new RestClientException(request.RequestUri, response.StatusCode,
                               "No deserializer found for " + contentTypeMediaType);

        try
        {
            return await deserializer.Deserialize(response.Content, type);
        }
        catch (Exception e)
        {
            if (e is RestClientException)
            {
                throw;
            }

            var errorResponse = await response.Content.ReadAsStringAsync();
            throw new RestClientException(request.RequestUri, response.StatusCode, errorResponse, e);
        }
    }
}