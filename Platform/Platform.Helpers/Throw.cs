using System;
using System.Runtime.CompilerServices;

namespace Platform.Helpers
{
    public static class Throw
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotSupportedException()
        {
            throw new NotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T NotSupportedExceptionAndReturn<T>()
        {
            throw new NotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotImplementedException()
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T NotImplementedExceptionAndReturn<T>()
        {
            throw new NotImplementedException();
        }
    }
}
