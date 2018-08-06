using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Caching.Memory;

namespace Activout.RestClient
{
    public interface IRestClientBuilder
    {
        IRestClientBuilder BaseUri(Uri apiUri);
        [Obsolete("HttpClient() method is deprecated, please use With() instead.")]
        IRestClientBuilder HttpClient(HttpClient httpClient);
        IRestClientBuilder With(HttpClient httpClient);
        IRestClientBuilder With(IResponseCache responseCache);
        IRestClientBuilder Header(string name, object value);
        IRestClientBuilder Header(AuthenticationHeaderValue authenticationHeaderValue);
        T Build<T>() where T : class;
    }
}