using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Activout.RestClient.DomainErrors
{
    public abstract class AbstractDomainErrorMapper : IDomainErrorMapper
    {
        public virtual Task<object> MapAsync(HttpResponseMessage httpResponseMessage, object data)
        {
            return Task.FromResult(Map(httpResponseMessage, data));
        }

        protected virtual object Map(HttpResponseMessage httpResponseMessage, object data)
        {
            throw new NotImplementedException();
        }
    }
}