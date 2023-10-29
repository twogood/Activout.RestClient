namespace Activout.RestClient.Implementation
{
    public static class StringExtensions
    {
        public static string SafeSubstring(this string value, int startIndex, int length)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            if (value.Length >= (startIndex + length))
            {
                return value.Substring(startIndex, length);
            }

            if (value.Length >= startIndex)
            {
                return value.Substring(startIndex);
            }

            return string.Empty;
        }
    }
}