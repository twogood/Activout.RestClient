namespace Activout.RestClient
{
    public interface IRestClientFactory
    {
        IRestClientBuilder CreateBuilder();
    }
}