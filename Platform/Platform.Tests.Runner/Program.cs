using System;
using System.IO;

using Platform.Examples;

namespace Platform.Data.Tests.Runner
{
    class Program
    {
        private const string DefaultTestsAssembly = "Platform.Tests.dll";

        static int Main(string[] args)
        {
            var directory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            var testsAssembly = Path.Combine(directory, DefaultTestsAssembly);

            args = new string[] { testsAssembly };

            var runner = new XUnitTestsRunnerCLI();
            runner.Run(args);
            return runner.Succeed ? 0 : 1;
        }
    }
}
