using System;
using System.Diagnostics;

namespace Platform.Helpers
{
    public class PerformanceHelpers
    {
        public static TimeSpan Measure(Action action)
        {
            var sw = Stopwatch.StartNew();

            action();

            sw.Stop();
            return sw.Elapsed;
        }
    }
}
