using System;

namespace Platform.Helpers
{
    public static class RandomExtensions
    {
        public static ulong NextUInt64(this Random rnd, ulong minValue = ulong.MinValue, ulong maxValue = ulong.MaxValue)
        {
            if (minValue >= maxValue)
                return minValue;
            return (ulong)(rnd.NextDouble() * (maxValue - minValue)) + minValue;
        }
    }
}