﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Activout.RestClient.Serialization.Implementation
{
    internal class StringDeserializer : IDeserializer
    {
        public MediaTypeCollection SupportedMediaTypes => TextCommon.SupportedMediaTypes;

        public async Task<object> Deserialize(HttpContent content, Type type)
        {
            if (type != typeof(string))
            {
                throw new NotImplementedException("Only string return value supported, but was " + type);
            }

            return await content.ReadAsStringAsync();
        }
    }
}