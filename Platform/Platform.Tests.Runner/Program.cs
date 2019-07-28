using System.IO;
using Platform.Examples;

namespace Platform.Data.Tests.Runner
{
    class Program
    {
        private const string DefaultTestsAssembly = "Platform.Tests.dll";

        static int Main()
        {
            var directory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            var testsAssembly = Path.Combine(directory, DefaultTestsAssembly);
            var runner = new XUnitTestsRunnerCLI();
            runner.Run(new string[] { testsAssembly });
            return runner.Succeed ? 0 : 1;
        }
    }
}
