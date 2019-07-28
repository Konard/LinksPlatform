using System.Reflection;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

using Platform.Data.Triplets;

namespace Platform.Data.WebTerminal
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var databaseFile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), @"data.dat");
#if DEBUG
            File.Delete(databaseFile);
#endif
            Link.StartMemoryManager(databaseFile);
            CreateWebHostBuilder(args).Build().Run();
            Link.StopMemoryManager();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) => WebHost.CreateDefaultBuilder(args).UseStartup<Startup>();
    }
}
