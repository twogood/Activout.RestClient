using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Activout.RestClient.DomainExceptions;
using Activout.RestClient.Helpers;
using Activout.RestClient.Helpers.Implementation;
using Activout.RestClient.ParamConverter;
using Activout.RestClient.ParamConverter.Implementation;
using Activout.RestClient.Serialization;
using Activout.RestClient.Serialization.Implementation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Activout.RestClient.Implementation;

internal class RestClientBuilder : IRestClientBuilder
{
    private static readonly MediaType DefaultContentType = new MediaType("text/plain");

    private IDuckTyping _duckTyping = DuckTyping.Instance;

    private readonly List<ISerializer>
        _serializers = new List<ISerializer>(); // SerializationManager.DefaultSerializers.ToList();

    private readonly List<IDeserializer>
        _deserializers = new List<IDeserializer>(); // SerializationManager.DefaultDeserializers.ToList();

    private ILogger? _logger;
    private Uri? _baseUri;
    private string _baseTemplate = "";
    private ISerializer? _defaultSerializer;
    private ISerializationManager? _serializationManager;
    private HttpClient? _httpClient;
    private ITaskConverterFactory? _taskConverterFactory;
    private Type? _errorResponseType;
    private MediaType? _defaultContentType;
    private IParamConverterManager? _paramConverterManager;
    private readonly List<KeyValuePair<string, object>> _defaultHeaders = new List<KeyValuePair<string, object>>();
    private IRequestLogger? _requestLogger;
    private Type? _domainExceptionType;
    private IDomainExceptionMapperFactory? _domainExceptionMapperFactory;

    public IRestClientBuilder BaseUri(Uri apiUri)
    {
        _baseUri = AddTrailingSlash(apiUri);
        return this;
    }

    public IRestClientBuilder ContentType(MediaType contentType)
    {
        _defaultContentType = contentType;
        return this;
    }

    public IRestClientBuilder Header(string name, object value, bool isReplace = true)
    {
        if (string.Equals("content-type", name, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Use ContentType method to set default content type.");
        }
        _defaultHeaders.AddOrReplaceHeader(name, value, isReplace);
        return this;
    }

    public IRestClientBuilder With(IDuckTyping duckTyping)
    {
        _duckTyping = duckTyping;
        return this;
    }

    public IRestClientBuilder With(ILogger logger)
    {
        _logger = logger;
        return this;
    }

    public IRestClientBuilder With(HttpClient httpClient)
    {
        _httpClient = httpClient;
        return this;
    }

    public IRestClientBuilder With(IRequestLogger requestLogger)
    {
        _requestLogger = requestLogger;
        return this;
    }

    public IRestClientBuilder With(IDeserializer deserializer)
    {
        if (_serializationManager != null)
        {
            throw new InvalidOperationException(
                "Cannot add custom deserializers when a custom SerializationManager has been set.");
        }

        _deserializers.Add(deserializer);
        return this;
    }

    public IRestClientBuilder With(ISerializer serializer)
    {
        if (_serializationManager != null)
        {
            throw new InvalidOperationException(
                "Cannot add custom serializers when a custom SerializationManager has been set.");
        }

        _serializers.Add(serializer);
        return this;
    }

    public IRestClientBuilder With(ISerializationManager serializationManager)
    {
        if (_serializers.Count > 0 || _deserializers.Count > 0)
        {
            throw new InvalidOperationException(
                "Cannot set a custom SerializationManager when custom serializers or deserializers have been added.");
        }

        _serializationManager = serializationManager;
        return this;
    }

    public IRestClientBuilder With(ITaskConverterFactory taskConverterFactory)
    {
        _taskConverterFactory = taskConverterFactory;
        return this;
    }

    public IRestClientBuilder With(IDomainExceptionMapperFactory domainExceptionMapperFactory)
    {
        _domainExceptionMapperFactory = domainExceptionMapperFactory;
        return this;
    }

    public IRestClientBuilder With(IParamConverterManager paramConverterManager)
    {
        _paramConverterManager = paramConverterManager;
        return this;
    }

    public T Build<T>() where T : class
    {
        var type = typeof(T);
        HandleAttributes(type);

        _defaultContentType ??= DefaultContentType;
        _serializationManager ??= new SerializationManager(
            _serializers.Concat(SerializationManager.DefaultSerializers).ToList(),
            _deserializers.Concat(SerializationManager.DefaultDeserializers).ToList());
        _defaultSerializer ??= _serializationManager.GetSerializer(_defaultContentType) ??
                               throw new InvalidOperationException(
                                   $"No serializer found for default content type {_defaultContentType}");

        var context = new RestClientContext(
            BaseUri: _baseUri ?? throw new InvalidOperationException("BaseUri is not set."),
            BaseTemplate: _baseTemplate,
            DefaultSerializer: _defaultSerializer,
            SerializationManager: _serializationManager,
            HttpClient: _httpClient ?? new HttpClient(),
            TaskConverterFactory: _taskConverterFactory ?? TaskConverter3Factory.Instance,
            ErrorResponseType: _errorResponseType,
            DefaultContentType: _defaultContentType ?? throw new InvalidOperationException("DefaultContentType is not set."),
            ParamConverterManager: _paramConverterManager ?? ParamConverterManager.Instance,
            DomainExceptionType: _domainExceptionType,
            DomainExceptionMapperFactory: _domainExceptionMapperFactory ??
                                          new DefaultDomainExceptionMapperFactory(),
            DefaultHeaders: new List<KeyValuePair<string, object>>(_defaultHeaders),
            Logger: _logger ?? NullLogger.Instance,
            RequestLogger: _requestLogger ?? DummyRequestLogger.Instance
        );

        var client = new RestClient(type, context);
        return _duckTyping.DuckType<T>(client);
    }

    private void HandleAttributes(Type type)
    {
        var attributes = type.GetCustomAttributes();
        foreach (var attribute in attributes)
            switch (attribute)
            {
                case ContentTypeAttribute contentTypeAttribute:
                    _defaultContentType = MediaType.ValueOf(contentTypeAttribute.ContentType);
                    break;
                case DomainExceptionAttribute domainExceptionAttribute:
                    _domainExceptionType = domainExceptionAttribute.Type;
                    break;
                case ErrorResponseAttribute errorResponseAttribute:
                    _errorResponseType = errorResponseAttribute.Type;
                    break;
                case HeaderAttribute headerAttribute:
                    _defaultHeaders.AddOrReplaceHeader(headerAttribute.Name, headerAttribute.Value,
                        headerAttribute.Replace);
                    break;
                case PathAttribute pathAttribute:
                    if (pathAttribute.Template != null)
                    {
                        _baseTemplate = pathAttribute.Template;
                    }

                    break;
            }
    }

    private static Uri AddTrailingSlash(Uri apiUri)
    {
        var uriBuilder = new UriBuilder(apiUri ?? throw new ArgumentNullException(nameof(apiUri)));
        if (uriBuilder.Path.EndsWith('/'))
        {
            return apiUri;
        }

        uriBuilder.Path += "/";
        return uriBuilder.Uri;
    }
}