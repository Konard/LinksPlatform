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

        public static T And<T>(T x, T y) => MathHelpers<T>.And(x, y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Increment<T>(T x) => MathHelpers<T>.Increment(x);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Subtract<T>(T x, T y) => MathHelpers<T>.Subtract(x, y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Subtract<T>(Integer<T> x, Integer<T> y) => MathHelpers<T>.Subtract(x, y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Decrement<T>(T x) => MathHelpers<T>.Decrement(x);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEquals<T>(T x, T y) => MathHelpers<T>.IsEquals(x, y);

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T PartialWrite<T>(T target, T source, int shift, int limit) => MathHelpers<T>.PartialWrite(target, source, shift, limit);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T PartialRead<T>(T target, int shift, int limit) => MathHelpers<T>.PartialRead(target, shift, limit);
    }

    public static class MathHelpers<T>
    {
        /// <remarks>
        /// TODO: Возможно, если компиляцию производить не в делегат, а в объект с методом, то оптимизация компилятором будет работать лучше.
        /// TODO: Возможно использовать ссылку на существующий метод - оператор, а не компилировать новый (http://stackoverflow.com/questions/11113259/how-to-call-custom-operator-with-reflection)
        /// TODO: Решить что лучше dynamic operator или Auto&lt;T&gt; c заранее созданными операторами и возможность расширения через статические методы
        /// </remarks>
        public static readonly Func<T, T, T> Add;
        public static readonly Func<T, T, T> And;
        public static readonly Func<T, T> Increment;
        public static readonly Func<T, T, T> Subtract;
        public static readonly Func<T, T> Decrement;
        public static readonly Func<T, T, bool> IsEquals;
        public static readonly Func<T, T, bool> GreaterThan;
        public static readonly Func<T, T, bool> GreaterOrEqualThan;
        public static readonly Func<T, T, bool> LessThan;
        public static readonly Func<T, T, bool> LessOrEqualThan;
        public static readonly Func<T, T> Abs;
        public static readonly Func<T, T> Negate;
        public static readonly Func<T, T, int, int, T> PartialWrite;
        public static readonly Func<T, int, int, T> PartialRead;

        static MathHelpers()
        {
            DelegateHelpers.Compile(out Add, emiter =>
            {
                EnsureNumeric();

                emiter.LoadArguments(0, 1);
                emiter.Add();
                emiter.Return();
            });

            DelegateHelpers.Compile(out And, emiter =>
            {
                EnsureNumeric();

                emiter.LoadArguments(0, 1);
                emiter.And();
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

            DelegateHelpers.Compile(out IsEquals, emiter =>
            {
                EnsureCanBeNumeric();

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

            DelegateHelpers.Compile(out GreaterOrEqualThan, emiter =>
            {
                EnsureNumeric();

                var secondIsGreaterOrEqual = emiter.DefineLabel();
                var theEnd = emiter.DefineLabel();

                emiter.LoadArguments(0, 1);
                emiter.BranchIfGreaterOrEqual(CachedTypeInfo<T>.IsSigned, secondIsGreaterOrEqual);
                emiter.LoadConstant(false);
                emiter.Branch(theEnd);
                emiter.MarkLabel(secondIsGreaterOrEqual);
                emiter.LoadConstant(true);
                emiter.MarkLabel(theEnd);
                emiter.Return();
            });

            DelegateHelpers.Compile(out LessThan, emiter =>
            {
                EnsureNumeric();

                emiter.LoadArguments(0, 1);
                emiter.CompareLessThan(CachedTypeInfo<T>.IsSigned);
                emiter.Return();
            });

            DelegateHelpers.Compile(out LessOrEqualThan, emiter =>
            {
                EnsureNumeric();

                var secondIsLessOrEqual = emiter.DefineLabel();
                var theEnd = emiter.DefineLabel();

                emiter.LoadArguments(0, 1);
                emiter.BranchIfLessOrEqual(CachedTypeInfo<T>.IsSigned, secondIsLessOrEqual);
                emiter.LoadConstant(false);
                emiter.Branch(theEnd);
                emiter.MarkLabel(secondIsLessOrEqual);
                emiter.LoadConstant(true);
                emiter.MarkLabel(theEnd);
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
                EnsureSigned();

                emiter.LoadArgument(0);
                emiter.Negate();
                emiter.Return();
            });

            DelegateHelpers.Compile(out PartialWrite, emiter =>
            {
                EnsureNumeric();

                var constants = GetConstants<T>();
                var bitsNumber = constants.Item1;
                var numberFilledWithOnes = constants.Item2;

                ushort shiftArgument = 2;
                ushort limitArgument = 3;

                var checkLimit = emiter.DefineLabel();
                var calculateSourceMask = emiter.DefineLabel();

                    // Check shift
                    emiter.LoadArgument(shiftArgument);
                emiter.LoadConstant(0);
                emiter.BranchIfGreaterOrEqual(checkLimit); // Skip fix

                    // Fix shift
                    emiter.LoadConstant(bitsNumber);
                emiter.LoadArgument(shiftArgument);
                emiter.Add();
                emiter.StoreArgument(shiftArgument);

                emiter.MarkLabel(checkLimit);
                    // Check limit
                    emiter.LoadArgument(limitArgument);
                emiter.LoadConstant(0);
                emiter.BranchIfGreaterOrEqual(calculateSourceMask); // Skip fix

                    // Fix limit
                    emiter.LoadConstant(bitsNumber);
                emiter.LoadArgument(limitArgument);
                emiter.Add();
                emiter.StoreArgument(limitArgument);

                emiter.MarkLabel(calculateSourceMask);

                using (var sourceMask = emiter.DeclareLocal<T>())
                using (var targetMask = emiter.DeclareLocal<T>())
                {
                    emiter.LoadConstant(typeof(T), numberFilledWithOnes);
                    emiter.LoadArgument(limitArgument);
                    emiter.ShiftLeft();
                    emiter.Not();
                    emiter.LoadConstant(typeof(T), numberFilledWithOnes);
                    emiter.And();
                    emiter.StoreLocal(sourceMask);

                    emiter.LoadLocal(sourceMask);
                    emiter.LoadArgument(shiftArgument);
                    emiter.ShiftLeft();
                    emiter.Not();
                    emiter.StoreLocal(targetMask);

                    emiter.LoadArgument(0); // target
                        emiter.LoadLocal(targetMask);
                    emiter.And();
                    emiter.LoadArgument(1); // source
                        emiter.LoadLocal(sourceMask);
                    emiter.And();
                    emiter.LoadArgument(shiftArgument);
                    emiter.ShiftLeft();
                    emiter.Or();
                }

                emiter.Return();
            });

            DelegateHelpers.Compile(out PartialRead, emiter =>
            {
                EnsureNumeric();

                var constants = GetConstants<T>();
                var bitsNumber = constants.Item1;
                var numberFilledWithOnes = constants.Item2;

                ushort shiftArgument = 1;
                ushort limitArgument = 2;

                var checkLimit = emiter.DefineLabel();
                var calculateSourceMask = emiter.DefineLabel();

                    // Check shift
                    emiter.LoadArgument(shiftArgument);
                emiter.LoadConstant(0);
                emiter.BranchIfGreaterOrEqual(checkLimit); // Skip fix

                    // Fix shift
                    emiter.LoadConstant(bitsNumber);
                emiter.LoadArgument(shiftArgument);
                emiter.Add();
                emiter.StoreArgument(shiftArgument);

                emiter.MarkLabel(checkLimit);
                    // Check limit
                    emiter.LoadArgument(limitArgument);
                emiter.LoadConstant(0);
                emiter.BranchIfGreaterOrEqual(calculateSourceMask); // Skip fix

                    // Fix limit
                    emiter.LoadConstant(bitsNumber);
                emiter.LoadArgument(limitArgument);
                emiter.Add();
                emiter.StoreArgument(limitArgument);

                emiter.MarkLabel(calculateSourceMask);

                using (var sourceMask = emiter.DeclareLocal<T>())
                using (var targetMask = emiter.DeclareLocal<T>())
                {
                    emiter.LoadConstant(typeof(T), numberFilledWithOnes);
                    emiter.LoadArgument(limitArgument); // limit
                        emiter.ShiftLeft();
                    emiter.Not();
                    emiter.LoadConstant(typeof(T), numberFilledWithOnes);
                    emiter.And();
                    emiter.StoreLocal(sourceMask);

                    emiter.LoadLocal(sourceMask);
                    emiter.LoadArgument(shiftArgument);
                    emiter.ShiftLeft();
                    emiter.StoreLocal(targetMask);

                    emiter.LoadArgument(0); // target
                        emiter.LoadLocal(targetMask);
                    emiter.And();
                    emiter.LoadArgument(shiftArgument);
                    emiter.ShiftRight();
                }

                emiter.Return();
            });
        }

        private static void EnsureCanBeNumeric()
        {
            if (!CachedTypeInfo<T>.CanBeNumeric)
                throw new NotSupportedException();
        }

        private static void EnsureNumeric()
        {
            if (!CachedTypeInfo<T>.IsNumeric)
                throw new NotSupportedException();
        }

        private static void EnsureSigned()
        {
            if (!CachedTypeInfo<T>.IsSigned)
                throw new NotSupportedException();
        }

        private static Tuple<int, TElement> GetConstants<TElement>()
        {
            var type = typeof(T);
            if (type == typeof(ulong))
                return new Tuple<int, TElement>(64, (TElement)(object)ulong.MaxValue);
            if (type == typeof(uint))
                return new Tuple<int, TElement>(32, (TElement)(object)uint.MaxValue);
            if (type == typeof(ushort))
                return new Tuple<int, TElement>(16, (TElement)(object)ushort.MaxValue);
            if (type == typeof(byte))
                return new Tuple<int, TElement>(8, (TElement)(object)byte.MaxValue);
            throw new NotSupportedException();
        }
    }
}
