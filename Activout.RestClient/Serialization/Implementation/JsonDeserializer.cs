using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;

namespace Activout.RestClient.Serialization.Implementation
{
    class JsonDeserializer : IDeserializer
    {
        public MediaTypeCollection SupportedMediaTypes
        {
            get
            {
                return JsonHelper.SupportedMediaTypes;
            }
        }

        public async Task<object> Deserialize(HttpContent content, Type type)
        {
            return JsonConvert.DeserializeObject(await content.ReadAsStringAsync(), type);
        }
    }
}
