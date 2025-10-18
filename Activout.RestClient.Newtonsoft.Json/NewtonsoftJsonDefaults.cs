using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Activout.RestClient.Newtonsoft.Json;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class NewtonsoftJsonDefaults
{
    public static readonly MediaType[] SupportedMediaTypes =
    [
        MediaType.ValueOf("application/json")
    ];

    public static readonly JsonConverter[] DefaultJsonConverters = [new SimpleValueObjectConverter()];

    public static readonly DefaultContractResolver CamelCasePropertyNamesContractResolver = new DefaultContractResolver
    {
        NamingStrategy = new CamelCaseNamingStrategy()
    };

    public static readonly JsonSerializerSettings DefaultJsonSerializerSettings = new()
    {
        Converters = DefaultJsonConverters.ToList()
    };

    public static readonly JsonSerializerSettings CamelCaseSerializerSettings = new()
    {
        Converters = DefaultJsonConverters.ToList(),
        ContractResolver = CamelCasePropertyNamesContractResolver,
    };
}