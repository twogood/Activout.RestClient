using System;
using System.Net;

namespace Activout.RestClient
{
    public class RestClientException : Exception
    {
        public RestClientException(Uri requestUri, HttpStatusCode statusCode, object errorResponse) : base(errorResponse?.ToString())
        {
            RequestUri = requestUri;
            StatusCode = statusCode;
            ErrorResponse = errorResponse;
        }

        public RestClientException(Uri requestUri, HttpStatusCode statusCode, string errorResponse, Exception innerException) : base(
            errorResponse, innerException)
        {
            RequestUri = requestUri;
            StatusCode = statusCode;
            ErrorResponse = errorResponse;
        }

        public Uri RequestUri { get; }
        public HttpStatusCode StatusCode { get; }

        public object ErrorResponse { get; }

        public T GetErrorResponse<T>()
        {
            return (T)ErrorResponse;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(RequestUri)}: {RequestUri}, {nameof(StatusCode)}: {StatusCode}, {nameof(ErrorResponse)}: {ErrorResponse}";
        }
    }
}