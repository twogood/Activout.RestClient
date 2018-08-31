using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace Activout.RestClient.Serialization.Implementation
{
    internal class ByteArrayDeserializer : IDeserializer
    {
        public MediaTypeCollection SupportedMediaTypes => new MediaTypeCollection
        {
            new MediaTypeHeaderValue("application/octet-stream")
        };

        public async Task<object> Deserialize(HttpContent content, Type type)
        {
            var bytes = await content.ReadAsByteArrayAsync();

            return type == typeof(byte[])
                ? await content.ReadAsByteArrayAsync()
                : Activator.CreateInstance(type, bytes);
        }
    }
}