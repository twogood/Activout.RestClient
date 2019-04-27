using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace Activout.RestClient.DomainErrors
{
    public class DefaultDomainErrorMapper : AbstractDomainErrorMapper
    {
        private readonly Type _domainErrorType;
        private readonly IList<DomainHttpErrorAttribute> _attributes;

        public DefaultDomainErrorMapper(Type domainErrorType,
            IEnumerable<DomainHttpErrorAttribute> attributes)
        {
            _domainErrorType = domainErrorType;
            _attributes = attributes.ToList();
        }

        protected override object Map(HttpResponseMessage httpResponseMessage, object data)
        {
            return MapByDomainErrorAttribute(data)
                   ?? MapHttpStatusCodeByAttribute(httpResponseMessage)
                   ?? MapHttpStatusCodeByEnumName(httpResponseMessage)
                   ?? MapGenericClientOrServerError(httpResponseMessage);
        }

        private static object MapByDomainErrorAttribute(object data)
        {
            if (data == null)
            {
                return null;
            }

            return (from property in data.GetType().GetProperties()
                let attributes = property.GetCustomAttributes(true)
                    .Where(a => a.GetType() == typeof(DomainErrorAttribute))
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
            return _attributes
                .FirstOrDefault(x => x.HttpStatusCode == httpStatusCode)
                ?.DomainErrorValue;
        }

        private object MapGenericClientOrServerError(HttpResponseMessage httpResponseMessage)
        {
            var httpStatusCode = httpResponseMessage.StatusCode;
            return ((int) httpStatusCode > 500 ? GetDomainErrorValue((HttpStatusCode) 500) : null)
                   ?? ((int) httpStatusCode > 400 ? GetDomainErrorValue((HttpStatusCode) 400) : null);
        }
    }
}