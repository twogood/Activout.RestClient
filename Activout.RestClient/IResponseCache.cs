using System.Net.Http;

namespace Activout.RestClient
{
    public interface IResponseCache
    {
        object CreateKey(HttpRequestMessage request);
        bool TryGetValue(object key, out object value);
        bool TrySetValue(object key, object value, HttpResponseMessage response);
    }
}