using System;
using System.Text;

namespace Platform.Helpers
{
    public static class StringExtensions
    {
        public static string CapitalizeFirstLetter(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return str;
            return new StringBuilder().Append(char.ToUpper(str[0])).Append(str.Substring(1, str.Length - 1)).ToString();
        }

        public static string Truncate(this string str, int maxLength) => str.Substring(0, Math.Min(str.Length, maxLength));
    }
}