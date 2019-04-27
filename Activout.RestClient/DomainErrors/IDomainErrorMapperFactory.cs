using System;
using System.Collections.Generic;

namespace Activout.RestClient.DomainErrors
{
    public interface IDomainErrorMapperFactory
    {
        IDomainErrorMapper CreateDomainErrorMapper(Type errorResponseType, Type domainErrorType,
            IEnumerable<DomainHttpErrorAttribute> httpErrorAttributes);
    }
}