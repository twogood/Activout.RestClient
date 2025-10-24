#nullable disable
using System.Net.Http;

namespace Activout.RestClient.Test
{
    public class HttpRequestMessageCollector
    {
        public HttpRequestMessage? Message { get; set; }
    }
}