using System;
using System.Net.Http.Headers;

namespace Activout.RestClient
{
    public sealed class MediaType
    {
        public static MediaType? ValueOf(string? value)
        {
            return value == null ? null : new MediaType(value);
        }

        public string Value { get; }

        public MediaType(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            if (!MediaTypeHeaderValue.TryParse(Value, out _))
            {
                throw new ArgumentException(nameof(value), "Failed to parse MediaType: " + value);
            }
        }

        public override string ToString()
        {
            return Value;
        }

        private bool Equals(MediaType other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is MediaType other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}