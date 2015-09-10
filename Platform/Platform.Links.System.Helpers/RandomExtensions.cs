using System;

namespace Platform.Links.System.Helpers
{
    public static class RandomExtensions
    {
        public static ulong NextUInt64(this Random rnd)
        {
            return rnd.NextUInt64(UInt64.MaxValue);
        }

        public static ulong NextUInt64(this Random rnd, ulong maxValue)
        {
            return rnd.NextUInt64(UInt64.MinValue, maxValue);
        }

        public static ulong NextUInt64(this Random rnd, ulong minValue, ulong maxValue)
        {
            if (minValue >= maxValue)
                return minValue;
            return (ulong)(rnd.NextDouble() * (maxValue - minValue)) + minValue;
        }
    }
}
