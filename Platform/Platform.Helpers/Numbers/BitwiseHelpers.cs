namespace Platform.Helpers.Numbers
{
    public static class BitwiseHelpers
    {
        public static long CountBits(long x)
        {
            long n = 0;
            while (x != 0)
            {
                n++;
                x = x & x - 1;
            }
            return n;
        }

        public static int GetLowestBitPosition(ulong value)
        {
            if (value == 0)
                return -1;

            var position = 0;
            while ((value & 1UL) == 0)
            {
                value >>= 1;
                ++position;
            }
            return position;
        }

        public static T PartialWrite<T>(T target, T source, int shift, int limit) => BitwiseHelpers<T>.PartialWrite(target, source, shift, limit);

        public static T PartialRead<T>(T target, int shift, int limit) => BitwiseHelpers<T>.PartialRead(target, shift, limit);
    }
}
