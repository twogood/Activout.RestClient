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
            if (type != typeof(string))
            {
                throw new NotImplementedException(
                    $"Can only deserialize {content.Headers.ContentType} to string, not to {type}");
            }

            return await content.ReadAsStringAsync();
        }
    }
}