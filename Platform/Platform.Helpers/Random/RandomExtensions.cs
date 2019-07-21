using System.Runtime.CompilerServices;
using Platform.Ranges;

namespace Platform.Helpers.Random
{
    public static class RandomExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong NextUInt64(this System.Random rnd) => rnd.NextUInt64(new Range<ulong>(ulong.MinValue, ulong.MaxValue));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong NextUInt64(this System.Random rnd, Range<ulong> range) => (ulong)(rnd.NextDouble() * (range.Maximum - range.Minimum)) + range.Minimum;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NextBoolean(this System.Random rnd) => rnd.Next(2) == 1;
    }
}