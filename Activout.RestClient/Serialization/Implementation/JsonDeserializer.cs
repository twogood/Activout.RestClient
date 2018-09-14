using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Activout.RestClient.Serialization.Implementation
{
    public class JsonDeserializer : IDeserializer
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public MediaTypeCollection SupportedMediaTypes => JsonHelper.SupportedMediaTypes;

        public JsonDeserializer(JsonSerializerSettings jsonSerializerSettings)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
        }

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

            return JsonConvert.DeserializeObject(await content.ReadAsStringAsync(), type, _jsonSerializerSettings);
        }
    }
}