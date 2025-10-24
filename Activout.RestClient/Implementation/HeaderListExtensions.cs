#nullable disable
using System;
using System.Collections.Generic;

namespace Activout.RestClient.Implementation
{
    public static class HeaderListExtensions
    {
        public static void AddOrReplaceHeader(this List<KeyValuePair<string, object>> headers, string name,
            string value, bool replace)
        {
            if (replace)
            {
                headers.RemoveAll(header =>
                    header.Key.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            }

            headers.Add(new KeyValuePair<string, object>(name, value));
        }
    }
}