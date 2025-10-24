using Activout.RestClient.Implementation;

namespace Activout.RestClient;

public class RestClientFactory : IRestClientFactory
{
    public IRestClientBuilder CreateBuilder() => new RestClientBuilder();
}