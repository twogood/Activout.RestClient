using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;

namespace Activout.RestClient.Serialization.Implementation
{
    internal class JsonSerializer : ISerializer
    {
        public MediaTypeCollection SupportedMediaTypes => JsonHelper.SupportedMediaTypes;

        public HttpContent Serialize(object data, Encoding encoding, string mediaType)
        {
            return new StringContent(
                JsonConvert.SerializeObject(data),
                encoding, mediaType);
        }
    }
}