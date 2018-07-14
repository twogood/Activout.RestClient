using System;

namespace Activout.RestClient
{
    public interface IRestClientBuilder
    {
        IRestClientBuilder BaseUri(Uri apiUri);
        T Build<T>() where T : class;
    }
}
