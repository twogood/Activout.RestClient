#nullable disable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Activout.RestClient.DomainExceptions
{
    public class DefaultDomainExceptionMapperFactory : IDomainExceptionMapperFactory
    {
        public virtual IDomainExceptionMapper CreateDomainExceptionMapper(
            MethodInfo method,
            Type errorResponseType,
            Type exceptionType)
        {
            var errorProperty = exceptionType.GetProperty("Error") ??
                                throw new ArgumentException("Exception must have a property called Error",
                                    nameof(exceptionType));


            var errorType = errorProperty.PropertyType;
            CheckDomainErrorAttributes(errorResponseType, errorType);

            var httpErrorAttributes = GetAllDomainHttpErrorAttributes(method, errorType);
            return new DefaultDomainExceptionMapper(exceptionType, errorType, httpErrorAttributes);
        }

        private static IEnumerable<DomainHttpErrorAttribute> GetAllDomainHttpErrorAttributes(MemberInfo method,
            Type errorType)
        {
            var attributes = GetDomainHttpErrorAttributes(method).ToList();
            attributes.AddRange(GetDomainHttpErrorAttributes(method.DeclaringType));
            CheckDomainHttpErrorAttributes(errorType, attributes);
            return attributes;
        }

        private static IEnumerable<DomainHttpErrorAttribute> GetDomainHttpErrorAttributes(
            ICustomAttributeProvider customAttributeProvider)
        {
            return customAttributeProvider
                .GetCustomAttributes(typeof(DomainHttpErrorAttribute), true)
                .Cast<DomainHttpErrorAttribute>();
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


        private static void CheckDomainErrorAttributes(Type errorResponseType,
            Type domainErrorType)
        {
            var attributes = GetProperties(errorResponseType)
                .SelectMany(p => p.GetCustomAttributes(typeof(DomainErrorAttribute), true))
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

        [SuppressMessage("SonarCloud", "S1523")]
        private static IEnumerable<PropertyInfo> GetProperties(Type type)
        {
            return type.GetProperties();
        }
    }
}