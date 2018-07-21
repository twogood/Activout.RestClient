using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Activout.RestClient.Serialization.Implementation
{
    internal class JsonDeserializer : IDeserializer
    {
        public MediaTypeCollection SupportedMediaTypes => JsonHelper.SupportedMediaTypes;

        public async Task<object> Deserialize(HttpContent content, Type type)
        {
            if (type == typeof(JObject))
            {
                return JObject.Parse(await content.ReadAsStringAsync());
            }

            if (type == typeof(JArray))
            {
                return JArray.Parse(await content.ReadAsStringAsync());
            }

            return JsonConvert.DeserializeObject(await content.ReadAsStringAsync(), type);
        }
    }
}