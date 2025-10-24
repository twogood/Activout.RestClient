using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using Activout.RestClient.DomainExceptions;
using Activout.RestClient.Helpers;
using Activout.RestClient.ParamConverter;
using Activout.RestClient.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Activout.RestClient.Implementation;

internal record RestClientContext(
    ILogger Logger,
    Uri BaseUri,
    string BaseTemplate,
    ISerializer DefaultSerializer,
    ISerializationManager SerializationManager,
    HttpClient HttpClient,
    ITaskConverterFactory TaskConverterFactory,
    Type? ErrorResponseType,
    MediaType? DefaultContentType,
    IParamConverterManager ParamConverterManager,
    List<KeyValuePair<string, object>> DefaultHeaders,
    IRequestLogger RequestLogger,
    Type? DomainExceptionType,
    IDomainExceptionMapperFactory DomainExceptionMapperFactory
    )
{
    public bool UseDomainException => DomainExceptionType != null;
}

internal sealed class DummyRequestLogger : IRequestLogger, IDisposable
{
    public IDisposable TimeOperation(HttpRequestMessage httpRequestMessage)
    {
        return this;
    }

    public void Dispose()
    {
        // Do nothing
    }
}