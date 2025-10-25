using System;
using System.Collections.Generic;

namespace Activout.RestClient.Implementation;

public static class HeaderListExtensions
{
    public static void AddOrReplaceHeader<T>(this List<KeyValuePair<string, T>> headers, string name,
        T value, bool replace)
    {
        if (replace)
        {
            headers.RemoveAll(header =>
                header.Key.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        headers.Add(new KeyValuePair<string, T>(name, value));
    }
}