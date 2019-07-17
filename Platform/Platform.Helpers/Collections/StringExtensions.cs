using System;
using System.Globalization;

namespace Platform.Helpers.Collections
{
    public static class StringExtensions
    {
        public static string CapitalizeFirstLetter(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return str;

            var chars = str.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                var category = char.GetUnicodeCategory(chars[i]);
                if (category == UnicodeCategory.UppercaseLetter)
                    return str;
                if (category == UnicodeCategory.LowercaseLetter)
                {
                    chars[i] = char.ToUpper(chars[i]);
                    return new string(chars);
                }
            }
            return str;
        }

        public static string Truncate(this string str, int maxLength)
        {
            if (string.IsNullOrEmpty(str))
                return str;
            return str.Substring(0, Math.Min(str.Length, maxLength));
        }
    }
}