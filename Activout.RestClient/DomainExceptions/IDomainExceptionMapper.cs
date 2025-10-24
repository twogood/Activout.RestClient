#nullable disable
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Activout.RestClient.DomainExceptions
{
    public interface IDomainExceptionMapper
    {
        Task<Exception> CreateExceptionAsync(HttpResponseMessage httpResponseMessage, object data,
            Exception innerException = null);

    }
}