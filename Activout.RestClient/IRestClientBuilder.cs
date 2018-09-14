using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Activout.RestClient.Serialization;

namespace Activout.RestClient
{
    public interface IRestClientBuilder
    {
        IRestClientBuilder BaseUri(Uri apiUri);
        IRestClientBuilder HttpClient(HttpClient httpClient);
        IRestClientBuilder Header(string name, object value);
        IRestClientBuilder Header(AuthenticationHeaderValue authenticationHeaderValue);
        IRestClientBuilder With(IRequestLogger requestLogger);
        IRestClientBuilder With(IDeserializer deserializer);
        IRestClientBuilder With(ISerializer serializer);
        IRestClientBuilder With(ISerializationManager serializationManager);
        T Build<T>() where T : class;
    }
}