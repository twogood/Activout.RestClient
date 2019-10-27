using System;
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
            var stringData = await content.ReadAsStringAsync();
            return type == typeof(string)
                ? stringData
                : Activator.CreateInstance(type, stringData);
        }
    }
}