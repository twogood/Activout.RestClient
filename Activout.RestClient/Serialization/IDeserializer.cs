using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Activout.RestClient.Serialization
{
    public interface IDeserializer
    {
        MediaTypeCollection SupportedMediaTypes { get; }
        Task<object> Deserialize(HttpContent content, Type type);
    }
}