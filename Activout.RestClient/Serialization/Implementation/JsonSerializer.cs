using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace Activout.RestClient.Serialization.Implementation
{
    class JsonSerializer : ISerializer
    {
        public MediaTypeCollection SupportedMediaTypes
        {
            get
            {
                return JsonHelper.SupportedMediaTypes;
            }
        }

        public HttpContent Serialize(object data, Encoding encoding, string mediaType)
        {
            return new StringContent(
                    JsonConvert.SerializeObject(data),
                    encoding, mediaType);
        }
    }
}
