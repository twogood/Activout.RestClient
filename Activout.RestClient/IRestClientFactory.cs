using System.Net.Http;

namespace Activout.RestClient
{
    public interface IRestClientFactory
    {
        IRestClientBuilder CreateBuilder(HttpClient httpClient = null);
    }
}