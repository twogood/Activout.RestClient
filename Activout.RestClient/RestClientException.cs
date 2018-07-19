using System;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Activout.RestClient.Helpers;

namespace Activout.RestClient
{
    internal static class Extensions
    {
        public static T GetValue<T>(this SerializationInfo self, string name)
        {
            return (T) self.GetValue(name, typeof(T));
        }

        public static bool IsSerializable(this object obj)
        {
            if (obj == null)
                return false;

            var t = obj.GetType();
            return t.IsSerializable;
        }
    }

    [Serializable]
    public class RestClientException : Exception
    {
        public RestClientException(HttpStatusCode statusCode, object errorResponse) : base(errorResponse?.ToString())
        {
            StatusCode = statusCode;
            ErrorResponse = errorResponse;
        }

        public RestClientException(HttpStatusCode statusCode, string errorResponse, Exception innerException) : base(
            errorResponse, innerException)
        {
            StatusCode = statusCode;
            ErrorResponse = errorResponse;
        }

        protected RestClientException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            StatusCode = info.GetValue<HttpStatusCode>(nameof(StatusCode));
            var typeName = info.GetString(nameof(Type));
            if (typeName != null)
            {
                ErrorResponse = info.GetValue(nameof(ErrorResponse), Type.GetType(typeName));
            }
        }

        public HttpStatusCode StatusCode { get; }

        public object ErrorResponse { get; }

        public T GetErrorResponse<T>()
        {
            return (T) ErrorResponse;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Preconditions.CheckNotNull(info);

            info.AddValue(nameof(StatusCode), StatusCode);
            if (ErrorResponse.IsSerializable())
            {
                info.AddValue(nameof(Type), ErrorResponse.GetType().AssemblyQualifiedName);
                info.AddValue(nameof(ErrorResponse), ErrorResponse);
            }
            else
            {
                info.AddValue(nameof(Type), null);
            }

            base.GetObjectData(info, context);
        }

        public override string ToString()
        {
            return $"{base.ToString()}; StatusCode={StatusCode}; ErrorMessage={ErrorResponse}";
        }
    }
}