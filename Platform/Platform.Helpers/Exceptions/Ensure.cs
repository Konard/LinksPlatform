using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Platform.Helpers.Collections;

namespace Platform.Helpers.Exceptions
{
    /// <remarks>
    /// Shorter version of ExceptionHelpers.
    /// Укороченная версия от ExceptionHelpers.
    /// </remarks>
    public static class Ensure
    {
        [Conditional("DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ArgumentNotNull<TArgument>(TArgument argument, string argumentName)
            where TArgument : class
        {
            if (argument == null)
                throw new ArgumentNullException(argumentName);
        }

        [Conditional("DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ArgumentNotEmpty<TArgument>(TArgument[] argument, string argumentName)
        {
            if (argument.IsNullOrEmpty())
                throw new ArgumentNullException(argumentName);
        }

        [Conditional("DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ArgumentZeroOrPositive(long argument, string argumentName)
        {
            if (argument < 0)
                throw new ArgumentOutOfRangeException(argumentName, "Must be positive.");
        }

        [Conditional("DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ArgumentInRange<T>(T argument, Range<T> range)
            where T : IComparable<T>
        {
            ArgumentInRange(argument, null, range);
        }

        [Conditional("DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ArgumentInRange<T>(T argument, string argumentName, Range<T> range)
            where T : IComparable<T>
        {
            if (!range.ContainsValue(argument))
                throw new ArgumentOutOfRangeException(argumentName, $"{argument} is out of range {range}.");
        }

        [Conditional("DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotDisposed(Disposables.IDisposable disposable, string objectName)
        {
            if (disposable.IsDisposed)
                throw new ObjectDisposedException(objectName);
        }
    }
}
