using System;
using System.Collections.Generic;
using Xunit;
using Platform.Helpers;

namespace Platform.Tests
{
    public static class EqualityTests
    {
        protected class UInt64EqualityComparer : IEqualityComparer<ulong>
        {
            public bool Equals(ulong x, ulong y) => x == y;

            public int GetHashCode(ulong obj) => obj.GetHashCode();
        }

        private static bool Equals1<T>(T x, T y) => Equals(x, y);

        private static bool Equals2<T>(T x, T y) => x.Equals(y);

        private static bool Equals3(ulong x, ulong y) => x == y;

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

            var equalityComparer1 = EqualityComparer<ulong>.Default;

            var ts4 = PerformanceHelpers.Measure(() =>
            {
                for (int i = 0; i < N; i++)
                    result = equalityComparer1.Equals(x, y);
            });

            var equalityComparer2 = new UInt64EqualityComparer();

            var ts5 = PerformanceHelpers.Measure(() =>
            {
                for (int i = 0; i < N; i++)
                    result = equalityComparer2.Equals(x, y);
            });

            Func<ulong, ulong, bool> equalityComparer3 = equalityComparer2.Equals;

            var ts6 = PerformanceHelpers.Measure(() =>
            {
                for (int i = 0; i < N; i++)
                    result = equalityComparer3(x, y);
            });

            var comparer = Comparer<ulong>.Default;

            var ts7 = PerformanceHelpers.Measure(() =>
            {
                for (int i = 0; i < N; i++)
                    result = comparer.Compare(x, y) == 0;
            });

            Assert.True(ts2 < ts1);
            Assert.True(ts3 < ts2);
            Assert.True(ts5 < ts4);
            Assert.True(ts5 < ts6);

            Console.WriteLine($"{ts1} {ts2} {ts3} {ts4} {ts5} {ts6} {ts7} {result}");
        }
    }
}
