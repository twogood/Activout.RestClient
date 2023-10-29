using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;

namespace Activout.RestClient.DomainExceptions
{
    public class DefaultDomainExceptionMapper : AbstractDomainExceptionMapper
    {
        private readonly Type _domainExceptionType;
        private readonly Type _domainErrorType;
        private readonly IList<DomainHttpErrorAttribute> _httpErrorAttributes;

        public DefaultDomainExceptionMapper(
            Type domainExceptionType,
            Type domainErrorType,
            IEnumerable<DomainHttpErrorAttribute> httpErrorAttributes)
        {
            _domainExceptionType = domainExceptionType;
            _domainErrorType = domainErrorType;
            _httpErrorAttributes = httpErrorAttributes.ToList();
        }

        protected override Exception CreateException(HttpResponseMessage httpResponseMessage, object data,
            Exception innerException)
        {
            var domainError = MapByDomainErrorAttribute(data)
                              ?? MapHttpStatusCodeByAttribute(httpResponseMessage)
                              ?? MapHttpStatusCodeByEnumName(httpResponseMessage)
                              ?? MapGenericClientOrServerError(httpResponseMessage);

            try
            {
                return (Exception)Activator.CreateInstance(_domainExceptionType, domainError, innerException);
            }
            catch (MissingMethodException)
            {
                return (Exception)Activator.CreateInstance(_domainExceptionType, domainError);
            }
        }

        private static object MapByDomainErrorAttribute(object data)
        {
            if (data == null)
            {
                return null;
            }

            return (from property in GetProperties(data.GetType())
                    let attributes = property.GetCustomAttributes(typeof(DomainErrorAttribute), true)
                        .Cast<DomainErrorAttribute>()
                        .ToList()
                    where attributes.Any()
                    let value = property.GetValue(data)
                    from a in attributes
                    where a.ApiValue.Equals(value)
                    select a.DomainValue).FirstOrDefault();
        }

        private object MapHttpStatusCodeByEnumName(HttpResponseMessage httpResponseMessage)
        {
            try
            {
                return _domainErrorType.IsEnum
                    ? Enum.Parse(_domainErrorType, httpResponseMessage.StatusCode.ToString())
                    : null;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private object MapHttpStatusCodeByAttribute(HttpResponseMessage httpResponseMessage)
        {
            return GetDomainErrorValue(httpResponseMessage.StatusCode);
        }

        private object GetDomainErrorValue(HttpStatusCode httpStatusCode)
        {
            return _httpErrorAttributes
                .FirstOrDefault(x => x.HttpStatusCode == httpStatusCode)
                ?.DomainErrorValue;
        }

        private object MapGenericClientOrServerError(HttpResponseMessage httpResponseMessage)
        {
            var httpStatusCode = httpResponseMessage.StatusCode;
            return ((int)httpStatusCode > 500 ? GetDomainErrorValue((HttpStatusCode)500) : null)
                   ?? ((int)httpStatusCode > 400 ? GetDomainErrorValue((HttpStatusCode)400) : null);
        }

        [SuppressMessage("SonarCloud", "S1523")]
        private static IEnumerable<PropertyInfo> GetProperties(Type type)
        {
            return type.GetProperties();
        }
    }
}