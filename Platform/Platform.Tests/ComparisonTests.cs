using System;
using System.Collections.Generic;
using Xunit;
using Platform.Helpers;

namespace Platform.Tests
{
    public static class ComparisonTests
    {
        protected class UInt64Comparer : IComparer<ulong>
        {
            public int Compare(ulong x, ulong y) => x.CompareTo(y);
        }

        private static int Compare(ulong x, ulong y) => x.CompareTo(y);

        [Fact]
        public static void GreaterOrEqualPerfomanceTest()
        {
            const int N = 1000000;

            ulong x = 10;
            ulong y = 500;

            bool result = false;

            var ts1 = PerformanceHelpers.Measure(() =>
            {
                for (int i = 0; i < N; i++)
                    result = Compare(x, y) >= 0;
            });

            var comparer1 = Comparer<ulong>.Default;

            var ts2 = PerformanceHelpers.Measure(() =>
            {
                for (int i = 0; i < N; i++)
                    result = comparer1.Compare(x, y) >= 0;
            });

            Func<ulong, ulong, int> compareReference = comparer1.Compare;

            var ts3 = PerformanceHelpers.Measure(() =>
            {
                for (int i = 0; i < N; i++)
                    result = compareReference(x, y) >= 0;
            });

            var comparer2 = new UInt64Comparer();

            var ts4 = PerformanceHelpers.Measure(() =>
            {
                for (int i = 0; i < N; i++)
                    result = comparer2.Compare(x, y) >= 0;
            });

            Console.WriteLine($"{ts1} {ts2} {ts3} {ts4} {result}");
        }
    }
}
