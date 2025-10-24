using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Net.Http;
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
    private IDuckTyping _duckTyping = DuckTyping.Instance;
    private readonly List<ISerializer> _serializers = SerializationManager.DefaultSerializers.ToList();
    private readonly List<IDeserializer> _deserializers = SerializationManager.DefaultDeserializers.ToList();
    private ILogger _logger = NullLogger.Instance;
    private Uri? _baseUri;
    private string? _baseTemplate;
    private ISerializer? _defaultSerializer = null;
    private ISerializationManager? _serializationManager;
    private HttpClient? _httpClient;
    private ITaskConverterFactory? _taskConverterFactory;
    private Type? _errorResponseType;
    private MediaType? _defaultContentType;
    private IParamConverterManager? _paramConverterManager;
    private readonly List<KeyValuePair<string, object>> _defaultHeaders = new List<KeyValuePair<string, object>>();
    private IRequestLogger _requestLogger = new DummyRequestLogger();
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

    public IRestClientBuilder Header(string name, object value)
    {
        _defaultHeaders.Add(new KeyValuePair<string, object>(name, value));
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
        _deserializers.Add(deserializer);
        return this;
    }

    public IRestClientBuilder With(ISerializer serializer)
    {
        _serializers.Add(serializer);
        return this;
    }

    public IRestClientBuilder With(ISerializationManager serializationManager)
    {
        _serializationManager = serializationManager;
        return this;
    }

    public IRestClientBuilder With(ITaskConverterFactory taskConverterFactory)
    {
        _taskConverterFactory = taskConverterFactory;
        return this;
    }

    public IRestClientBuilder With(IParamConverterManager paramConverterManager)
    {
        _paramConverterManager = paramConverterManager;
        return this;
    }

    public IRestClientBuilder With(IDomainExceptionMapperFactory domainExceptionMapperFactory)
    {
        _domainExceptionMapperFactory = domainExceptionMapperFactory;
        return this;
    }

    public IRestClientBuilder With(IDuckTyping duckTyping)
    {
        _duckTyping = duckTyping;
        return this;
    }

    private void HandleAttributes(Type type)
    {
        var attributes = type.GetCustomAttributes();
        foreach (var attribute in attributes)
            switch (attribute)
            {
                case ContentTypeAttribute contentTypeAttribute:
                    _defaultContentType = new MediaType(contentTypeAttribute.ContentType);
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
                    _baseTemplate = pathAttribute.Template;
                    break;
            }
    }

    public T Build<T>() where T : class
    {
        _serializationManager ??= new SerializationManager(_serializers, _deserializers);

        var type = typeof(T);
        HandleAttributes(type);

        var client = new RestClient(type, new RestClientContext(
                Logger: _logger,
                BaseUri: _baseUri ?? throw new InvalidOperationException("BaseUri is not set."),
                BaseTemplate: _baseTemplate ?? "",
                DefaultSerializer: _defaultSerializer ?? new StringSerializer(),
                SerializationManager: _serializationManager,
                HttpClient: _httpClient ?? new HttpClient(),
                TaskConverterFactory: _taskConverterFactory ?? TaskConverter3Factory.Instance,
                ErrorResponseType: _errorResponseType,
                DefaultContentType: _defaultContentType,
                ParamConverterManager: _paramConverterManager ?? ParamConverterManager.Instance,
                DefaultHeaders: _defaultHeaders,
                RequestLogger: _requestLogger,
                DomainExceptionType: _domainExceptionType,
                DomainExceptionMapperFactory: _domainExceptionMapperFactory ??
                                              DefaultDomainExceptionMapperFactory.Instance
            )
        );
        return _duckTyping.DuckType<T>(client);
    }

    private static Uri AddTrailingSlash(Uri apiUri)
    {
        var uriBuilder = new UriBuilder(apiUri ?? throw new ArgumentNullException(nameof(apiUri)));
        if (uriBuilder.Path.EndsWith("/"))
        {
            return apiUri;
        }

        uriBuilder.Path = uriBuilder.Path + "/";
        return uriBuilder.Uri;
    }
}