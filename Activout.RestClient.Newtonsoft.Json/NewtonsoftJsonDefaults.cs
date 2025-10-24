using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Activout.RestClient.Newtonsoft.Json;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class NewtonsoftJsonDefaults
{
    public static readonly MediaType[] SupportedMediaTypes =
    [
        new MediaType("application/json")
    ];

    public static readonly JsonConverter[] DefaultJsonConverters =
    [
        new IsoDateTimeConverter(),
        new SimpleValueObjectConverter(),
    ];

    public static readonly DefaultContractResolver CamelCasePropertyNamesContractResolver = new DefaultContractResolver
    {
        NamingStrategy = new CamelCaseNamingStrategy(false, false)
    };

    public static readonly JsonSerializerSettings DefaultJsonSerializerSettings = new()
    {
        Converters = [new StringEnumConverter(), ..DefaultJsonConverters],
        NullValueHandling = NullValueHandling.Ignore
    };

    public static readonly JsonSerializerSettings CamelCaseSerializerSettings = new()
    {
        Converters =
        [
            new StringEnumConverter(CamelCasePropertyNamesContractResolver.NamingStrategy!),
            ..DefaultJsonConverters
        ],
        ContractResolver = CamelCasePropertyNamesContractResolver,
        NullValueHandling = NullValueHandling.Ignore
    };
}