#nullable disable
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Activout.RestClient.Serialization.Implementation
{
    internal class StringDeserializer : IDeserializer
    {
        public int Order { get; set; } = 1000;

        public async Task<object> Deserialize(HttpContent content, Type type)
        {
            var stringData = await content.ReadAsStringAsync();
            return type == typeof(string)
                ? stringData
                : Activator.CreateInstance(type, stringData);
        }

        public bool CanDeserialize(MediaType mediaType)
        {
            return mediaType.Value.StartsWith("text/");
        }
    }
}