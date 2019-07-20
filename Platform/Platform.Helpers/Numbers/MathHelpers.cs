namespace Platform.Helpers.Numbers
{
    /// <remarks>
    /// Resizable array (FileMappedMemory) for values cache may be used. or cached oeis.org
    /// </remarks>
    public class MathHelpers
    {
        /// <remarks>
        /// Source: https://oeis.org/A000142/list
        /// </remarks>
        private static readonly ulong[] Factorials =
        {
            1, 1, 2, 6, 24, 120, 720, 5040, 40320, 362880, 3628800, 39916800,
            479001600, 6227020800, 87178291200, 1307674368000, 20922789888000,
            355687428096000, 6402373705728000, 121645100408832000, 2432902008176640000
        };

        /// <remarks>
        /// Source: https://oeis.org/A000108/list
        /// </remarks>
        private static readonly ulong[] Catalans =
        {
            1, 1, 2, 5, 14, 42, 132, 429, 1430, 4862, 16796, 58786, 208012,
            742900, 2674440, 9694845, 35357670, 129644790, 477638700, 1767263190,
            6564120420, 24466267020, 91482563640, 343059613650, 1289904147324, 4861946401452,
            18367353072152, 69533550916004, 263747951750360, 1002242216651368, 3814986502092304
        };

        public static double Factorial(double n)
        {
            if (n <= 1) return 1;
            if (n < Factorials.Length) return Factorials[(int)n];

            return n * Factorial(n - 1);
        }

        public static double Catalan(double n)
        {
            if (n <= 1) return 1;
            if (n < Catalans.Length) return Catalans[(int)n];

            return Factorial(2 * n) / (Factorial(n + 1) * Factorial(n));
        }

        public static bool IsPowerOfTwo(ulong x) => (x & x - 1) == 0;

        public static T Abs<T>(T x) => MathHelpers<T>.Abs(x);

        public static T Negate<T>(T x) => MathHelpers<T>.Negate(x);
    }
}
