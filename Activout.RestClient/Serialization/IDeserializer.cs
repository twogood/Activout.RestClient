using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Activout.RestClient.Serialization
{
    public interface IDeserializer
    {
        MediaTypeCollection SupportedMediaTypes { get; }
        Task<object> Deserialize(HttpContent content, Type type);
    }
}
