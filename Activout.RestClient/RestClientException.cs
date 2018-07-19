using System;
using System.Net;

namespace Activout.RestClient
{
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

        public HttpStatusCode StatusCode { get; }

        public object ErrorResponse { get; }

        public T GetErrorResponse<T>()
        {
            return (T) ErrorResponse;
        }

        public override string ToString()
        {
            return $"{base.ToString()}; StatusCode={StatusCode}; ErrorMessage={ErrorResponse}";
        }
    }
}