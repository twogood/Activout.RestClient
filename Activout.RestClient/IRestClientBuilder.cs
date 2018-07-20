using System;
using System.Net.Http;

namespace Activout.RestClient
{
    public interface IRestClientBuilder
    {
        IRestClientBuilder BaseUri(Uri apiUri);
        IRestClientBuilder HttpClient(HttpClient httpClient);
        T Build<T>() where T : class;
    }
}