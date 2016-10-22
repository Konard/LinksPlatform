using System;
using System.IO;
using System.Threading;
using Platform.Helpers;
using Xunit.Runners;

namespace Platform.Examples
{
    public class TestsRunnerCLI : ICommandLineInterface
    {
        private const string DefaultAssembly = "Platform.Tests";

        // We use consoleLock because messages can arrive in parallel, so we want to make sure we get
        // consistent console output.
        private object consoleLock = new object();

        // Use an event to know when we're done
        private readonly ManualResetEvent finished = new ManualResetEvent(false);

        public void Run(params string[] args)
        {
            var location = System.Reflection.Assembly.GetEntryAssembly().Location;
            var directory = Path.GetDirectoryName(location);

            string testAssembly;
            if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
                testAssembly = args[0];
            else
                testAssembly = $"{Path.Combine(directory)}{Path.DirectorySeparatorChar}{DefaultAssembly}.dll";

            var typeName = args.Length >= 2 && !string.IsNullOrWhiteSpace(args[1]) ? args[1] : null;

            using (var runner = AssemblyRunner.WithoutAppDomain(testAssembly))
            {
                runner.OnDiscoveryComplete = OnDiscoveryComplete;
                runner.OnExecutionComplete = OnExecutionComplete;
                runner.OnTestFailed = OnTestFailed;
                runner.OnTestSkipped = OnTestSkipped;

                Console.WriteLine("Discovering...");
                runner.Start(typeName);

                finished.WaitOne();
                finished.Dispose();
            }

            ConsoleHelpers.PressAnyKeyToContinue();
        }

        private void OnDiscoveryComplete(DiscoveryCompleteInfo info)
        {
            lock (consoleLock)
                Console.WriteLine($"Running {info.TestCasesToRun} of {info.TestCasesDiscovered} tests...");
        }

        private void OnExecutionComplete(ExecutionCompleteInfo info)
        {
            lock (consoleLock)
                Console.WriteLine($"Finished: {info.TotalTests} tests in {Math.Round(info.ExecutionTime, 3)}s ({info.TestsFailed} failed, {info.TestsSkipped} skipped)");

            finished.Set();
        }

        private void OnTestFailed(TestFailedInfo info)
        {
            lock (consoleLock)
            {
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine("[FAIL] {0}: {1}", info.TestDisplayName, info.ExceptionMessage);
                if (info.ExceptionStackTrace != null)
                    Console.WriteLine(info.ExceptionStackTrace);

                Console.ResetColor();
            }
        }

        private void OnTestSkipped(TestSkippedInfo info)
        {
            lock (consoleLock)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[SKIP] {0}: {1}", info.TestDisplayName, info.SkipReason);
                Console.ResetColor();
            }
        }
    }
}
