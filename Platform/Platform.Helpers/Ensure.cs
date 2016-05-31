using System;
using System.Runtime.CompilerServices;
using Platform.Helpers.Collections;

namespace Platform.Helpers
{
    /// <remarks>
    /// Shorter version of ExceptionHelpers.
    /// Укороченная версия от ExceptionHelpers.
    /// </remarks>
    public static class Ensure
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ArgumentNotNull<TArgument>(TArgument argument, string argumentName)
            where TArgument : class
        {
            if (argument == null)
                throw new ArgumentNullException(argumentName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ArgumentNotEmpty<TArgument>(TArgument[] argument, string argumentName)
        {
            if (argument.IsNullOrEmpty())
                throw new ArgumentNullException(argumentName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ArgumentPositive(int argument, string argumentName)
        {
            if (argument < 0)
                throw new ArgumentOutOfRangeException(argumentName, "Must be positive.");
        }
    }
}
