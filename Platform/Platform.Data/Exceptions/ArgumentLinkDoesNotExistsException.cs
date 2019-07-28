using System;

namespace Platform.Data.Exceptions
{
    /// <remarks>
    /// TODO: Multilingual support for messages formats.
    /// </remarks>
    public class ArgumentLinkDoesNotExistsException<TLink> : ArgumentException
    {
        public ArgumentLinkDoesNotExistsException(TLink link, string paramName) : base(FormatMessage(link, paramName), paramName) { }

        public ArgumentLinkDoesNotExistsException(TLink link) : base(FormatMessage(link)) { }

        private static string FormatMessage(TLink link, string paramName) => $"Связь [{link}] переданная в аргумент [{paramName}] не существует.";

        private static string FormatMessage(TLink link) => $"Связь [{link}] переданная в качестве аргумента не существует.";
    }
}