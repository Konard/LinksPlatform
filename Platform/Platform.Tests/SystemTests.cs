using System;
using System.Diagnostics;
using Xunit;
using Platform.Helpers;
using Platform.Helpers.Numbers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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

        protected class UInt64EqualityComparer : IEqualityComparer<ulong>
        {
            public bool Equals(ulong x, ulong y) => x == y;

            public int GetHashCode(ulong obj) => obj.GetHashCode();
        }

        [Fact]
        public static void EqualsPerfomanceTest()
        {
            const int N = 1000000;

            ulong x = 10;
            ulong y = 500;

            bool result = false;

            var ts1 = PerformanceHelpers.Measure(() =>
            {
                for (int i = 0; i < N; i++)
                    result = Equals1(x, y);
            });

            var ts2 = PerformanceHelpers.Measure(() =>
            {
                for (int i = 0; i < N; i++)
                    result = Equals2(x, y);
            });

            var ts3 = PerformanceHelpers.Measure(() =>
            {
                for (int i = 0; i < N; i++)
                    result = Equals3(x, y);
            });

            if (!result)
                result = MathHelpers<ulong>.IsEquals(x, y); // Ensure precompiled

            var ts4 = PerformanceHelpers.Measure(() =>
            {
                for (int i = 0; i < N; i++)
                    result = MathHelpers<ulong>.IsEquals(x, y);
            });

            var equalityComparer1 = EqualityComparer<ulong>.Default;

            var ts5 = PerformanceHelpers.Measure(() =>
            {
                for (int i = 0; i < N; i++)
                    result = equalityComparer1.Equals(x, y);
            });

            var equalityComparer2 = new UInt64EqualityComparer();

            var ts6 = PerformanceHelpers.Measure(() =>
            {
                for (int i = 0; i < N; i++)
                    result = equalityComparer2.Equals(x, y);
            });

            Func<ulong, ulong, bool> equalityComparer3 = equalityComparer2.Equals;

            var ts7 = PerformanceHelpers.Measure(() =>
            {
                for (int i = 0; i < N; i++)
                    result = equalityComparer3(x, y);
            });

            var comparer = Comparer<ulong>.Default;

            var ts8 = PerformanceHelpers.Measure(() =>
            {
                for (int i = 0; i < N; i++)
                    result = comparer.Compare(x, y) == 0;
            });

            Assert.True(ts2 < ts1);
            Assert.True(ts3 < ts2);
            Assert.True(ts4 < ts2);
            Assert.True(ts6 < ts4);
            Assert.True(ts6 < ts5);
            Assert.True(ts6 < ts7);

            Console.WriteLine($"{ts1} {ts2} {ts3} {ts4} {ts5} {ts6} {ts7} {ts8} {result}");
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
