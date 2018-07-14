using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Activout.RestClient.Serialization.Implementation
{
    class JsonHelper
    {
        public static MediaTypeCollection SupportedMediaTypes { get; private set; }

        static JsonHelper()
        {
            SupportedMediaTypes = new MediaTypeCollection()
            {
                new MediaTypeHeaderValue("application/json")
            };
        }
    }
}
