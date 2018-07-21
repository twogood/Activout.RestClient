using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace Activout.RestClient.Serialization.Implementation
{
    internal static class TextCommon
    {
        static TextCommon()
        {
            SupportedMediaTypes = new MediaTypeCollection
            {
                new MediaTypeHeaderValue("text/*")
            };
        }

        public static MediaTypeCollection SupportedMediaTypes { get; }
    }
}