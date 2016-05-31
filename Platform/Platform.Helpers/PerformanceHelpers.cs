using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Platform.Helpers
{
    public class PerformanceHelpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan Measure(Action action)
        {
            var sw = Stopwatch.StartNew();

            action();

            sw.Stop();
            return sw.Elapsed;
        }
    }
}
