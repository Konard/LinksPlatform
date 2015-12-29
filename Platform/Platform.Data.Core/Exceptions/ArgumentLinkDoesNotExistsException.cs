using System;

namespace Platform.Data.Core.Exceptions
{
    /// <remarks>
    /// TODO: Multilingual support for messages formats.
    /// </remarks>
    public class ArgumentLinkDoesNotExistsException<TLink> : ArgumentException
    {
        public ArgumentLinkDoesNotExistsException(TLink link, string paramName)
            : base(FormatMessage(link, paramName), paramName)
        {
        }

        public ArgumentLinkDoesNotExistsException(TLink link)
            : base(FormatMessage(link))
        {
        }

        private static string FormatMessage(TLink link, string paramName)
        {
            return string.Format("Связь [{0}] переданная в аргумент [{1}] не существует.", link, paramName);
        }

        private static string FormatMessage(TLink link)
        {
            return string.Format("Связь [{0}] переданная в качестве аргумента не существует.", link);
        }
    }
}