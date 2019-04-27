using System;
using System.Collections.Generic;
using System.Linq;

namespace Activout.RestClient.DomainErrors
{
    public class DefaultDomainErrorMapperFactory : IDomainErrorMapperFactory
    {
        public virtual IDomainErrorMapper CreateDomainErrorMapper(Type errorResponseType, Type domainErrorType,
            IEnumerable<DomainHttpErrorAttribute> httpErrorAttributes)
        {
            var attributes = httpErrorAttributes.ToList();
            CheckDomainErrorAttributes(errorResponseType, domainErrorType);
            CheckDomainHttpErrorAttributes(domainErrorType, attributes);
            return new DefaultDomainErrorMapper(domainErrorType, attributes);
        }

        private static void CheckDomainHttpErrorAttributes(Type domainErrorType,
            IEnumerable<DomainHttpErrorAttribute> domainHttpErrorAttributes)
        {
            foreach (var domainHttpErrorAttribute in domainHttpErrorAttributes)
            {
                if (!domainErrorType.IsInstanceOfType(domainHttpErrorAttribute.DomainErrorValue))
                {
                    throw new InvalidOperationException(
                        $"Cannot assign value {domainHttpErrorAttribute.DomainErrorValue} to type {domainErrorType} for HTTP Status Code {domainHttpErrorAttribute.HttpStatusCode}");
                }
            }
        }


        private static void CheckDomainErrorAttributes(Type errorResponseType, Type domainErrorType)
        {
            var attributes = errorResponseType
                .GetCustomAttributes(true)
                .Where(a => a.GetType() == typeof(DomainErrorAttribute))
                .Cast<DomainErrorAttribute>()
                .ToList();

            foreach (var a in attributes)
            {
                if (!domainErrorType.IsInstanceOfType(a.DomainValue))
                {
                    throw new InvalidOperationException(
                        $"Cannot assign domain value {a.DomainValue} to type {domainErrorType} for API value {a.ApiValue}");
                }
            }
        }
    }
}