using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Platform.Helpers.Reflection;
using System.Reflection;

// ReSharper disable StaticFieldInGenericType

namespace Platform.Helpers
{
    /// <remarks>
    /// Resizable array (FileMappedMemory) for values cache may be used. or cached oeis.org
    /// </remarks>
    public class MathHelpers
    {
        // Source: https://oeis.org/A000142/list
        private static readonly long[] Factorials =
        {
            1,1,2,6,24,120,720,5040,40320,362880,3628800,
            39916800,479001600,6227020800,87178291200,
            1307674368000,20922789888000,355687428096000,
            6402373705728000,121645100408832000,
            2432902008176640000 /*51090942171709440000,
            1124000727777607680000 */
        };

        // Source: https://oeis.org/A000108/list
        private static readonly long[] Catalans =
        {
            1,1,2,5,14,42,132,429,1430,4862,16796,58786,
            208012,742900,2674440,9694845,35357670,129644790,
            477638700,1767263190,6564120420,24466267020,
            91482563640,343059613650,1289904147324,
            4861946401452,18367353072152,69533550916004,
            263747951750360,1002242216651368,3814986502092304
        };

        public static double Factorial(double n)
        {
            if (n <= 1) return 1;
            if (n < Factorials.Length) return Factorials[(long)n];

            return n * Factorial(n - 1);
        }

        public static double Catalan(double n)
        {
            if (n <= 1) return 1;
            if (n < Catalans.Length) return Catalans[(long)n];

            return Factorial(2 * n) / (Factorial(n + 1) * Factorial(n));
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(ulong x) => (x & (x - 1)) == 0;

        public static long CountBits(long x)
        {
            long n = 0;
            while (x != 0)
            {
                n++;
                x = x & (x - 1);
            }
            return n;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Add<T>(T x, T y) => MathHelpers<T>.Add(x, y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Increment<T>(T x) => MathHelpers<T>.Increment(x);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Subtract<T>(T x, T y) => MathHelpers<T>.Subtract(x, y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Subtract<T>(Integer<T> x, Integer<T> y) => MathHelpers<T>.Subtract(x, y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Decrement<T>(T x) => MathHelpers<T>.Decrement(x);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equals<T>(T x, T y) => MathHelpers<T>.Equals(x, y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GreaterThan<T>(T x, T y) => MathHelpers<T>.GreaterThan(x, y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GreaterOrEqualThan<T>(T x, T y) => MathHelpers<T>.GreaterOrEqualThan(x, y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LessThan<T>(T x, T y) => MathHelpers<T>.LessThan(x, y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LessOrEqualThan<T>(T x, T y) => MathHelpers<T>.LessOrEqualThan(x, y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Abs<T>(T x) => MathHelpers<T>.Abs(x);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Negate<T>(T x) => MathHelpers<T>.Negate(x);
    }

    public static class MathHelpers<T>
    {
        /// <remarks>
        /// TODO: Возможно, если компиляцию производить не в делегат, а в объект с методом, то оптимизация компилятором будет работать лучше.
        /// TODO: Возможно использовать ссылку на существующий метод - оператор, а не компилировать новый (http://stackoverflow.com/questions/11113259/how-to-call-custom-operator-with-reflection)
        /// TODO: Решить что лучше dynamic operator или Auto&lt;T&gt; c заранее созданными операторами и возможность расширения через статические методы
        /// </remarks>
        private static class CompiledOperations
        {
            public static readonly Func<T, T, T> Add;
            public static readonly Func<T, T> Increment;
            public static readonly Func<T, T, T> Subtract;
            public static readonly Func<T, T> Decrement;
            public new static readonly Func<T, T, bool> Equals;
            public static readonly Func<T, T, bool> GreaterThan;
            public static readonly Func<T, T, bool> LessThan;
            public static readonly Func<T, T> Abs;
            public static readonly Func<T, T> Negate;

            static CompiledOperations()
            {
                DelegateHelpers.Compile(out Add, emiter =>
                {
                    EnsureNumeric();

                    emiter.LoadArguments(0, 1);
                    emiter.Add();
                    emiter.Return();
                });

                DelegateHelpers.Compile(out Increment, emiter =>
                {
                    EnsureNumeric();

                    emiter.LoadArgument(0);
                    emiter.Increment(typeof(T));
                    emiter.Return();
                });

                DelegateHelpers.Compile(out Subtract, emiter =>
                {
                    EnsureNumeric();

                    emiter.LoadArguments(0, 1);
                    emiter.Subtract();
                    emiter.Return();
                });

                DelegateHelpers.Compile(out Decrement, emiter =>
                {
                    EnsureNumeric();

                    emiter.LoadArgument(0);
                    emiter.Decrement(typeof(T));
                    emiter.Return();
                });

                DelegateHelpers.Compile(out Equals, emiter =>
                {
                    EnsureNumeric();

                    emiter.LoadArguments(0, 1);
                    emiter.CompareEqual();
                    emiter.Return();
                });

                DelegateHelpers.Compile(out GreaterThan, emiter =>
                {
                    EnsureNumeric();

                    emiter.LoadArguments(0, 1);
                    emiter.CompareGreaterThan(CachedTypeInfo<T>.IsSigned);
                    emiter.Return();
                });

                DelegateHelpers.Compile(out LessThan, emiter =>
                {
                    EnsureNumeric();

                    emiter.LoadArguments(0, 1);
                    emiter.CompareLessThan(CachedTypeInfo<T>.IsSigned);
                    emiter.Return();
                });

                DelegateHelpers.Compile(out Abs, emiter =>
                {
                    EnsureNumeric();

                    emiter.LoadArgument(0);

                    if (CachedTypeInfo<T>.IsSigned)
                        emiter.Call(typeof(Math).GetTypeInfo().GetMethod("Abs", new[] { typeof(T) }));
                   
                    emiter.Return();
                });

                DelegateHelpers.Compile(out Negate, emiter =>
                {
                    EnsureNumeric();

                    emiter.LoadArgument(0);
                    emiter.Negate();
                    emiter.Return();
                });
            }

            private static void EnsureNumeric()
            {
                if (!CachedTypeInfo<T>.IsNumeric)
                    throw new NotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Add(T x, T y) => CompiledOperations.Add(x, y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Increment(T x) => CompiledOperations.Increment(x);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Subtract(T x, T y) => CompiledOperations.Subtract(x, y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Decrement(T x) => CompiledOperations.Decrement(x);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equals(T x, T y) => CompiledOperations.Equals(x, y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GreaterThan(T x, T y) => CompiledOperations.GreaterThan(x, y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GreaterOrEqualThan(T x, T y) => Equals(x, y) || GreaterThan(x, y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LessThan(T x, T y) => CompiledOperations.LessThan(x, y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LessOrEqualThan(T x, T y) => Equals(x, y) || LessThan(x, y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Abs(T x) => CompiledOperations.Abs(x);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Negate(T x) => CompiledOperations.Negate(x);
    }
}
