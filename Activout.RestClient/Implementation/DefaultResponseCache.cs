using System;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.Caching.Memory;

namespace Activout.RestClient.Implementation
{
    public class DefaultResponseCache : IResponseCache
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheEntryOptions Options { get; set; }

        public DefaultResponseCache(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public object CreateKey(HttpRequestMessage request)
        {
            return request.RequestUri + ":" + request.Headers;
        }

        public bool TryGetValue(object key, out object value)
        {
            return _memoryCache.TryGetValue(key, out value);
        }

        public bool TrySetValue(object key, object value, HttpResponseMessage response)
        {
            var currentOptions = Options;
            var checkExpires = true;

            var cacheControl = response.Headers.CacheControl;
            if (cacheControl != null)
            {
                if (cacheControl.NoCache || cacheControl.NoStore)
                {
                    return false;
                }

                if (cacheControl.MaxAge != null)
                {
                    currentOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = cacheControl.MaxAge
                    };
                    checkExpires = false;
                }
            }

            if (checkExpires && response.Content.Headers.Expires != null)
            {
                currentOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = response.Content.Headers.Expires
                };
            }

            _memoryCache.Set(key, value, currentOptions);
            return true;
        }
    }
}