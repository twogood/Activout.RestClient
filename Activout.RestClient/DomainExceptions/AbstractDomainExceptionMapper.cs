using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Activout.RestClient.DomainExceptions;

public abstract class AbstractDomainExceptionMapper : IDomainExceptionMapper
{
    public virtual Task<Exception> CreateExceptionAsync(HttpResponseMessage httpResponseMessage, object data,
        Exception innerException = null)
    {
        return Task.FromResult(CreateException(httpResponseMessage, data, innerException));
    }

    protected virtual Exception CreateException(HttpResponseMessage httpResponseMessage, object data,
        Exception innerException)
    {
        throw new NotImplementedException();
    }
}