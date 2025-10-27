using System;
using System.Collections.Generic;
using System.Net.Http;
using Activout.RestClient.DomainExceptions;
using Activout.RestClient.Helpers;
using Activout.RestClient.ParamConverter;
using Activout.RestClient.Serialization;
using Microsoft.Extensions.Logging;

namespace Activout.RestClient.Implementation;

internal record RestClientContext(
    Uri BaseUri,
    string BaseTemplate,
    ISerializer DefaultSerializer,
    ISerializationManager SerializationManager,
    HttpClient HttpClient,
    ITaskConverterFactory TaskConverterFactory,
    Type? ErrorResponseType,
    MediaType DefaultContentType,
    IParamConverterManager ParamConverterManager,
    Type? DomainExceptionType,
    IDomainExceptionMapperFactory DomainExceptionMapperFactory,
    IReadOnlyList<KeyValuePair<string, object>> DefaultHeaders,
    ILogger Logger,
    IRequestLogger RequestLogger)
{
    public bool UseDomainException => DomainExceptionType != null;
}