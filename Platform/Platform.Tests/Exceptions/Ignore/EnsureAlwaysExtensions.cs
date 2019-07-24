using System.Diagnostics;
using Platform.Exceptions;

namespace Platform.Tests.Exceptions.Ignore
{
    public static class EnsureAlwaysExtensions
    {
        [Conditional("DEBUG")]
        public static void ArgumentNotNull<TArgument>(this EnsureAlwaysExtensionRoot root, TArgument argument, string argumentName)
            where TArgument : class
        {
        }
    }
}
