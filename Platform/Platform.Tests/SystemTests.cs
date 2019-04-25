using System;
using System.Diagnostics;
using Xunit;
using Platform.Helpers;

namespace Platform.Tests
{
    /// <summary>
    /// Contains tests for features of .NET Framework itself, that are required by current implementations.
    /// </summary>
    public class SystemTests
    {
        [Fact]
        public void UsingSupportsNullTest()
        {
            using (var disposable = null as IDisposable)
                Assert.True(disposable == null);
        }

        [Fact]
        public static void EqualsPerfomanceTest()
        {
            const int N = 1000000;

            ulong x = 10;
            ulong y = 500;

            bool result = false;

            var sw1 = Stopwatch.StartNew();

            for (int i = 0; i < N; i++)
                result = Equals1(x, y);

            sw1.Stop();

            var sw2 = Stopwatch.StartNew();

            for (int i = 0; i < N; i++)
                result = Equals2(x, y);

            sw2.Stop();

            var sw3 = Stopwatch.StartNew();

            for (int i = 0; i < N; i++)
                result = Equals3(x, y);

            sw3.Stop();

            if (!result)
                result = MathHelpers<ulong>.IsEquals(x, y); // Ensure precompiled

            var sw4 = Stopwatch.StartNew();

            for (int i = 0; i < N; i++)
                result = MathHelpers<ulong>.IsEquals(x, y);

            sw4.Stop();

            Assert.True(sw2.Elapsed < sw1.Elapsed);
            Assert.True(sw3.Elapsed < sw2.Elapsed);
            Assert.True(sw4.Elapsed < sw2.Elapsed);

            Console.WriteLine($"{sw1.Elapsed} {sw2.Elapsed} {sw3.Elapsed} {sw4.Elapsed} {result}");
        }

        private static bool Equals1<T>(T x, T y) => Equals(x, y);

        private static bool Equals2<T>(T x, T y) => x.Equals(y);

        private static bool Equals3(ulong x, ulong y) => x == y;

        [Fact]
        public void PossiblePackTwoValuesIntoOneTest()
        {
            uint value = 0;

            // Set one to first bit

            value |= 1;

            Assert.True(value == 1);

            // Set zero to first bit
            value &= 0xFFFFFFFE;

            // Get first bit
            uint read = value & 1;

            Assert.True(read == 0);

            uint firstValue = 1;
            uint secondValue = 1543;

            // Pack (join) two values at the same time
            value = (secondValue << 1) | firstValue;

            uint unpackagedFirstValue = value & 1;
            uint unpackagedSecondValue = (value & 0xFFFFFFFE) >> 1;

            Assert.True(firstValue == unpackagedFirstValue);
            Assert.True(secondValue == unpackagedSecondValue);

            // Using universal functions:

            Assert.True(PartialRead(value, 0, 1) == firstValue);
            Assert.True(PartialRead(value, 1, -1) == secondValue);

            firstValue = 0;
            secondValue = 6892;

            value = PartialWrite(value, firstValue, 0, 1);
            value = PartialWrite(value, secondValue, 1, -1);

            Assert.True(PartialRead(value, 0, 1) == firstValue);
            Assert.True(PartialRead(value, 1, -1) == secondValue);
        }

        private uint PartialWrite(uint target, uint source, int shift, int limit)
        {
            if (shift < 0) shift = 32 + shift;
            if (limit < 0) limit = 32 + limit;
            var sourceMask = ~(uint.MaxValue << limit) & uint.MaxValue;
            var targetMask = ~(sourceMask << shift);
            return (target & targetMask) | ((source & sourceMask) << shift);
        }

        private uint PartialRead(uint target, int shift, int limit)
        {
            if (shift < 0) shift = 32 + shift;
            if (limit < 0) limit = 32 + limit;
            var sourceMask = ~(uint.MaxValue << limit) & uint.MaxValue;
            var targetMask = sourceMask << shift;
            return (target & targetMask) >> shift;
        }
    }
}
