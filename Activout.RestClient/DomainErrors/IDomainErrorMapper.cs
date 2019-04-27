using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Activout.RestClient.DomainErrors
{
    public interface IDomainErrorMapper
    {
        Task<object> MapAsync(HttpResponseMessage httpResponseMessage, object data);
    }
}