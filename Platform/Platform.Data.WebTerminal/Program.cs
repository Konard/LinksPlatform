using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Platform.Data.Core.Triplets;

namespace Platform.Data.WebTerminal
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var databaseFile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), @"data.dat");

            File.Delete(databaseFile);

            Link.StartMemoryManager(databaseFile);

            CreateWebHostBuilder(args).Build().Run();

            Link.StopMemoryManager();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
