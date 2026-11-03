using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Activout.RestClient.DomainExceptions;
using Activout.RestClient.ParamConverter;
using Activout.RestClient.Serialization;
using Microsoft.Extensions.Logging;

namespace Activout.RestClient.Implementation;

internal record HttpContentPart(HttpContent Content, string Name, string? FileName);

internal class RequestHandler
{
    // https://www.w3.org/Protocols/rfc2616/rfc2616-sec7.html#sec7.2.1
    private const string DefaultHttpContentType = "application/octet-stream";

    // https://tools.ietf.org/html/rfc7578#section-4.4
    private static readonly MediaType DefaultPartContentType = new MediaType("text/plain");

    private readonly RouteMetadata _meta;
    private readonly MediaType _contentType;
    private readonly RestClientContext _context;
    private readonly Type _errorResponseType;
    private readonly ISerializer _serializer;
    private readonly string _template;
    private readonly IParamConverter[] _paramConverters;
    private readonly IDomainExceptionMapper? _domainExceptionMapper;
    private readonly List<KeyValuePair<string, object>> _requestHeaders = new List<KeyValuePair<string, object>>();

    private bool IsDebugLoggingEnabled => _context.Logger.IsEnabled(LogLevel.Debug);

    public RequestHandler(MethodInfo method, RestClientContext context)
    {
        _meta = RouteMetadata.Get(method);
        _paramConverters = GetParamConverters(context.ParamConverterManager);
        if (!_meta.IsVoidTask && !_meta.IsGenericTask)
            context.Logger.LogWarning(
                "Synchronous method {Type}.{Method} is deprecated and will not be supported in the next major version. Return Task or Task<T> instead.",
                method.DeclaringType?.Name, method.Name);
        _template = context.BaseTemplate + _meta.TemplateSuffix;
        _contentType = _meta.ContentType ?? context.DefaultContentType;
        _errorResponseType = _meta.ErrorResponseType ?? context.ErrorResponseType ?? typeof(string);
        _requestHeaders.AddRange(context.DefaultHeaders);
        foreach (var header in _meta.Headers)
            _requestHeaders.AddOrReplaceHeader(header.Name, header.Value, header.Replace);

        _serializer = context.SerializationManager.GetSerializer(_contentType) ??
                      throw new InvalidOperationException("No serializer found for content type: " + _contentType);

        if (context.DomainExceptionType != null)
        {
            _domainExceptionMapper = context.DomainExceptionMapperFactory.CreateDomainExceptionMapper(
                method,
                _errorResponseType,
                context.DomainExceptionType);
        }

        _context = context;
    }

    private IParamConverter[] GetParamConverters(IParamConverterManager paramConverterManager)
    {
        var parameters = _meta.Parameters;
        var paramConverters = new IParamConverter[parameters.Length];
        for (var i = 0; i < parameters.Length; i++)
        {
            paramConverters[i] = paramConverterManager.GetConverter(parameters[i].ParameterType, parameters[i])
                                 ?? throw new InvalidOperationException(
                                     "No parameter converter found for type: " + parameters[i].ParameterType);
        }

        return paramConverters;
    }

    private string ExpandTemplate(Dictionary<string, object> routeParams)
    {
        var expanded = _template;
        foreach (var entry in routeParams)
        {
            expanded = expanded.Replace("{" + entry.Key + "}", entry.Value.ToString());
        }

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
        var partParams = new List<HttpContentPart>();
        var cancellationToken = GetParams(args, routeParams, queryParams, formParams, headers, partParams);

        var requestUriString = ExpandTemplate(routeParams);
        if (queryParams.Count != 0)
        {
            requestUriString = requestUriString + "?" + string.Join("&", queryParams);
        }

        var requestUri = new Uri(requestUriString, UriKind.RelativeOrAbsolute);

        var request = new HttpRequestMessage(_meta.HttpMethod, requestUri);

        SetHeaders(request, headers);

        if (_meta.IsHttpMethodWithBody)
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
                request.Content = GetHttpContent(_serializer, args[_meta.BodyArgumentIndex], _contentType);
            }
        }

        var task = SendRequestAndHandleResponse(request, cancellationToken);

        if (_meta.IsVoidTask)
            return task;
        if (_meta.IsGenericTask && _meta.Converter != null)
            return _meta.Converter(task);
        return task.Result;
    }

    private static MultipartFormDataContent CreateMultipartFormDataContent(
        IEnumerable<HttpContentPart> partParams)
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
        List<HttpContentPart> parts)
    {
        var cancellationToken = CancellationToken.None;

        var parameters = _meta.Parameters;
        if (parameters.Length > 0 && args == null || parameters.Length != args?.Length)
        {
            throw new InvalidOperationException(
                $"Argument count mismatch. Expected: {parameters.Length}, Actual: {args?.Length ?? 0}");
        }

        for (var i = 0; i < parameters.Length; i++)
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

            var parameterAttributes = _meta.ParameterAttributes[i];
            var parameterName = parameters[i].Name ?? throw new InvalidOperationException(
                "Parameter name not found for parameter at index: " + i);
            var handled = false;

            foreach (var attribute in parameterAttributes)
            {
                if (attribute is PartParamAttribute partAttribute)
                {
                    if (parameters[i].ParameterType.IsArray)
                    {
                        if (rawValue is IEnumerable items and not string)
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
                            var value = ConvertValueToString(entry.Value, parameters[i]);
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
                            var value = ConvertValueToString(entry.Value, parameters[i]);
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
                            var value = ConvertValueToString(entry.Value, parameters[i]);
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

    private IEnumerable<HttpContentPart> GetPartNameAndHttpContent(PartParamAttribute partAttribute,
        string parameterName,
        object? rawValue)
    {
        string? fileName = null;
        string? partName = null;

        if (rawValue is Part part)
        {
            rawValue = part.Content;
            partName = part.Name;
            fileName = part.FileName;
        }

        if (rawValue is not null)
        {
            yield return new HttpContentPart(
                Content: GetPartHttpContent(partAttribute, rawValue),
                Name: partName ?? partAttribute.Name ?? parameterName,
                FileName: fileName ?? partAttribute.FileName);
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


    private async Task<object?> SendRequestAndHandleResponse(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await SendRequest(request, cancellationToken).ConfigureAwait(false);
        return await HandleResponse(request, response).ConfigureAwait(false);
    }

    private async Task<HttpResponseMessage> SendRequest(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        PrepareRequestMessage(request);

        if (IsDebugLoggingEnabled)
        {
            _context.Logger.LogDebug("{Request}", request);

            if (request.Content != null)
            {
                await request.Content.LoadIntoBufferAsync().ConfigureAwait(false);
                _context.Logger.LogDebug("{RequestContent}",
                    (await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false)).SafeSubstring(0, 1000));
            }
        }

        HttpResponseMessage response;
        using (_context.RequestLogger.TimeOperation(request))
        {
            response = await _context.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        if (IsDebugLoggingEnabled)
        {
            _context.Logger.LogDebug("{Response}", response);

            await response.Content.LoadIntoBufferAsync().ConfigureAwait(false);
            _context.Logger.LogDebug("{ResponseContent}",
                (await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false)).SafeSubstring(0, 1000));
        }

        return response;
    }

    private async Task<object?> HandleResponse(HttpRequestMessage request, HttpResponseMessage response)
    {
        if (_meta.ActualReturnType == typeof(HttpResponseMessage))
        {
            return response;
        }

        var shouldDisposeResponse = true;
        try
        {
            if (_meta.ActualReturnType == typeof(HttpStatusCode))
            {
                return response.StatusCode;
            }

            object? data;
            var type = response.IsSuccessStatusCode ? _meta.ActualReturnType : _errorResponseType;

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
                data = await Deserialize(request, response, type).ConfigureAwait(false);
            }

            if (response.IsSuccessStatusCode)
            {
                return data;
            }

            if (_context.UseDomainException && _domainExceptionMapper != null)
            {
                throw await _domainExceptionMapper.CreateExceptionAsync(response, data).ConfigureAwait(false);
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
            return await deserializer.Deserialize(response.Content, type).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            if (e is RestClientException)
            {
                throw;
            }

            var errorResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            throw new RestClientException(request.RequestUri, response.StatusCode, errorResponse, e);
        }
    }
}