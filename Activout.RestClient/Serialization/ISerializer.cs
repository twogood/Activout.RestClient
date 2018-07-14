using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Activout.RestClient.Serialization
{
    public interface ISerializer
    {
        MediaTypeCollection SupportedMediaTypes { get; }
        HttpContent Serialize(object data, Encoding encoding, string mediaType);
    }
}
