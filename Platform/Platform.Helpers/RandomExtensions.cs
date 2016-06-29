using System;
using System.Runtime.CompilerServices;

namespace Platform.Helpers
{
    public static class RandomExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong NextUInt64(this Random rnd, ulong minValue = ulong.MinValue, ulong maxValue = ulong.MaxValue)
        {
            if (minValue >= maxValue)
                return minValue;
            return (ulong)(rnd.NextDouble() * (maxValue - minValue)) + minValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NextBoolean(this Random rnd)
        {
            return rnd.Next(2) == 1;
        }
    }
}