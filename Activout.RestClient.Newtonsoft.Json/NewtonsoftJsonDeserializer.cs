using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Activout.RestClient.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Activout.RestClient.Newtonsoft.Json;

public class NewtonsoftJsonDeserializer : IDeserializer
{
    private readonly JsonSerializerSettings _jsonSerializerSettings;

    public IReadOnlyCollection<MediaType> SupportedMediaTypes => JsonHelper.SupportedMediaTypes;

    public NewtonsoftJsonDeserializer(JsonSerializerSettings jsonSerializerSettings)
    {
        _jsonSerializerSettings = jsonSerializerSettings;
    }

    public int Order { get; set; }

    public async Task<object?> Deserialize(HttpContent content, Type type)
    {
        if (type == typeof(JObject))
        {
            return JObject.Parse(await content.ReadAsStringAsync());
        }

        if (type == typeof(JArray))
        {
            return JArray.Parse(await content.ReadAsStringAsync());
        }

        return JsonConvert.DeserializeObject(await content.ReadAsStringAsync(), type, _jsonSerializerSettings);
    }

    public bool CanDeserialize(MediaType mediaType)
    {
        return SupportedMediaTypes.Contains(mediaType);
    }
}