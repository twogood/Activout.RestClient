using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Activout.RestClient.Newtonsoft.Json;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class NewtonsoftJsonDefaults
{
    public static readonly MediaType[] SupportedMediaTypes =
    [
        MediaType.ValueOf("application/json")
    ];

    public static readonly JsonConverter[] DefaultJsonConverters = [new SimpleValueObjectConverter()];

    public static readonly JsonSerializerSettings DefaultJsonSerializerSettings = new()
    {
        Converters = DefaultJsonConverters.ToList()
    };
}