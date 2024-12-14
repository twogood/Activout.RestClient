namespace Activout.RestClient;

public interface IRestClientFactory
{
    IRestClientBuilder CreateBuilder();
    IRestClientBuilder Extend(IExtendable client);
}