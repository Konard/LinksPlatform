﻿using System;
using System.IO;
using System.Threading;
using Xunit.Runners;

namespace Platform.Examples
{
    /// <remarks>
    /// Based on https://github.com/xunit/samples.xunit/blob/master/TestRunner/Program.cs
    /// </remarks>
    public class XUnitTestsRunnerCLI : ICommandLineInterface
    {
        // We use consoleLock because messages can arrive in parallel, so we want to make sure we get
        // consistent console output.
        private readonly object consoleLock = new object();

        public bool Succeed { get; private set; }

        // Use an event to know when we're done
        private readonly ManualResetEvent finished = new ManualResetEvent(false);

        public XUnitTestsRunnerCLI()
        {
            Succeed = true;
        }

        public void Run(params string[] args)
        {
            var location = System.Reflection.Assembly.GetEntryAssembly().Location;

            string testAssembly;
            if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
                testAssembly = args[0];
            else
                testAssembly = location;

            var typeName = args.Length >= 2 && !string.IsNullOrWhiteSpace(args[1]) ? args[1] : null;

            using (var runner = AssemblyRunner.WithoutAppDomain(testAssembly))
            {
                runner.OnDiscoveryComplete = OnDiscoveryComplete;
                runner.OnExecutionComplete = OnExecutionComplete;
                runner.OnTestPassed = OnTestPassed;
                runner.OnTestFailed = OnTestFailed;
                runner.OnTestSkipped = OnTestSkipped;
                runner.OnErrorMessage = OnErrorMessage;

                Console.WriteLine("Discovering...");
                runner.Start(typeName);

                finished.WaitOne();
                finished.Dispose();

                if (runner.Status != AssemblyRunnerStatus.Idle)
                {
                    Console.WriteLine("Waiting to get Idle..");
                    while (runner.Status != AssemblyRunnerStatus.Idle)
                        Thread.Sleep(1);
                    Console.WriteLine("Idle status reached.");
                }
            }
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

        private void OnTestPassed(TestPassedInfo info)
        {
            lock (consoleLock)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[PASSED] {info.TestDisplayName}: {info.ExecutionTime}");
                Console.ResetColor();
            }
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

            Succeed = false;
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

        private void OnErrorMessage(ErrorMessageInfo info)
        {
            lock (consoleLock)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] {info.ExceptionType}: {info.ExceptionMessage}");
                Console.ResetColor();
            }

            Succeed = false;
            finished.Set();
        }
    }
}
