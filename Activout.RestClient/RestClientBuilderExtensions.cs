#nullable disable
using System;
using System.Net.Http.Headers;

namespace Activout.RestClient
{
    public static class RestClientBuilderExtensions
    {
        public static IRestClientBuilder BaseUri(this IRestClientBuilder self, string apiUri)
        {
            return self.BaseUri(new Uri(apiUri));
        }

        public static IRestClientBuilder ContentType(this IRestClientBuilder self, string contentType)
        {
            return self.ContentType(MediaType.ValueOf(contentType));
        }

        public static IRestClientBuilder Accept(this IRestClientBuilder self, string accept)
        {
            return self.Header("Accept", accept);
        }

        public static IRestClientBuilder Header(this IRestClientBuilder self,
            AuthenticationHeaderValue authenticationHeaderValue)
        {
            return self.Header("Authorization", authenticationHeaderValue);
        }
    }
}