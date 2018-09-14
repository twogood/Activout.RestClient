using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace Activout.RestClient.Serialization.Implementation
{
    public static class JsonHelper
    {
        static JsonHelper()
        {
            SupportedMediaTypes = new MediaTypeCollection
            {
                new MediaTypeHeaderValue("application/json")
            };
        }

        public static MediaTypeCollection SupportedMediaTypes { get; }
    }
}