using System;
using System.Net.Http.Headers;

namespace Activout.RestClient;

public record MediaType
{
    public static MediaType? ValueOf(string? value)
    {
        return value == null ? null : new MediaType(value);
    }

    public string Value { get; }

    public MediaType(string value)
    {
        Value = value;
        if (!MediaTypeHeaderValue.TryParse(Value, out _))
        {
            throw new ArgumentException("Failed to parse MediaType: " + value, nameof(value));
        }
    }

    public override string ToString()
    {
        return Value;
    }
}