#nullable disable
using System;
using System.Net.Http;
using Activout.RestClient.DomainExceptions;
using Activout.RestClient.Helpers;
using Activout.RestClient.Serialization;
using Microsoft.Extensions.Logging;

namespace Activout.RestClient;

public interface IRestClientBuilder
{
    IRestClientBuilder BaseUri(Uri apiUri);
    IRestClientBuilder ContentType(MediaType contentType);
    IRestClientBuilder Header(string name, object value, bool isReplace = false);
    IRestClientBuilder With(ILogger logger);
    IRestClientBuilder With(HttpClient httpClient);
    IRestClientBuilder With(IRequestLogger requestLogger);
    IRestClientBuilder With(IDeserializer deserializer);
    IRestClientBuilder With(ISerializer serializer);
    IRestClientBuilder With(ISerializationManager serializationManager);
    IRestClientBuilder With(ITaskConverterFactory taskConverterFactory);
    IRestClientBuilder With(IDomainExceptionMapperFactory domainExceptionMapperFactory);
    T Build<T>() where T : class;
}