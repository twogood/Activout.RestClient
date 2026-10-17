using Activout.RestClient.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Activout.RestClient.Newtonsoft.Json;

public class NewtonsoftJsonDeserializer : IDeserializer
{
    private readonly JsonSerializerSettings _jsonSerializerSettings;

    public IReadOnlyCollection<MediaType> SupportedMediaTypes => NewtonsoftJsonDefaults.SupportedMediaTypes;

    public NewtonsoftJsonDeserializer(JsonSerializerSettings jsonSerializerSettings)
    {
        _jsonSerializerSettings = jsonSerializerSettings;
    }

    public int Order { get; set; }

    public async Task<object?> Deserialize(HttpContent content, Type type)
    {
        if (type == typeof(JObject))
        {
            return JObject.Parse(await content.ReadAsStringAsync().ConfigureAwait(false));
        }

        if (type == typeof(JArray))
        {
            return JArray.Parse(await content.ReadAsStringAsync().ConfigureAwait(false));
        }

        return JsonConvert.DeserializeObject(await content.ReadAsStringAsync().ConfigureAwait(false), type, _jsonSerializerSettings);
    }

    public bool CanDeserialize(MediaType mediaType)
    {
        return SupportedMediaTypes.Contains(mediaType);
    }
}