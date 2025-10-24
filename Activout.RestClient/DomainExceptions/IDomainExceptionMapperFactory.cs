#nullable disable
using System;
using System.Reflection;

namespace Activout.RestClient.DomainExceptions
{
    public interface IDomainExceptionMapperFactory
    {
        IDomainExceptionMapper CreateDomainExceptionMapper(
            MethodInfo method,
            Type errorResponseType,
            Type exceptionType);
    }
}