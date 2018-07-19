using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;

namespace Activout.RestClient.Serialization.Implementation
{
    internal class JsonDeserializer : IDeserializer
    {
        public MediaTypeCollection SupportedMediaTypes => JsonHelper.SupportedMediaTypes;

        public async Task<object> Deserialize(HttpContent content, Type type)
        {
            return JsonConvert.DeserializeObject(await content.ReadAsStringAsync(), type);
        }
    }
}