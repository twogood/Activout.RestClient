using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Activout.RestClient.Serialization.Implementation
{
    internal class ByteArrayDeserializer : IDeserializer
    {
        public IReadOnlyCollection<MediaType> SupportedMediaTypes => new[]
        {
            MediaType.ValueOf("application/octet-stream")
        };

        public int Order { get; set; }

        public async Task<object> Deserialize(HttpContent content, Type type)
        {
            var bytes = await content.ReadAsByteArrayAsync();

            return type == typeof(byte[])
                ? bytes
                : Activator.CreateInstance(type, bytes);
        }

        public bool CanDeserialize(MediaType mediaType)
        {
            return SupportedMediaTypes.Contains(mediaType);
        }
    }
}