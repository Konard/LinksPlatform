using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

using NetLibrary;
using Utils;
using System.Diagnostics;

namespace ConsoleTester
{
	class Program
	{
		static Program()
		{

		}

		static void Main(string[] args)
		{
			Stopwatch sw = Stopwatch.StartNew();

			Link.StartMemoryManager(@"C:\data.dat");

			Console.WriteLine("Start time: {0}", sw.Elapsed);
			Console.WriteLine();

			//Console.WriteLine(Net.Letter.ToString());

			//UnusedLinksTest();

			//Net.Recreate();

			//var l = LinkConverter.FromString("latin alphabet");

			//CompressionDifferenceTests();

			//ThreadHelpers.SyncInvokeWithExtendedStack(() =>
			//    {
			//        FileReadWriteTest.Run();
			//    });

			//JitExperiments.RunExperiment();

			//TreeStructureExperiments.RunExperiment();

			//Console.WriteLine(Net.Link.Target.GetPointer());

			//Transactions.Run();

			try
			{
				TerminalExperiment.Run();

				//OperationsExperiments.RunExperiment();
			}
			catch (Exception ex)
			{
			}


			//NewMemoryManagerExperiments.RunExperiment2();

			//NetParserTest.Run();

			Console.WriteLine();

			Link.StopMemoryManager();
			
			Console.WriteLine("Total time: {0}", sw.Elapsed);
			Console.WriteLine();

			Console.WriteLine("All jobs done.");
			Console.ReadKey();
		}

		static void CompressionDifferenceTests()
		{
			string path = Console.ReadLine();
			XmlGenerator.ToFile(path);
		}

		static void UnusedLinksTest()
		{
			int unusedLinksCounter = 0;

			Link.WalkThroughAllLinks((link) =>
				{
					if (link.ReferersBySourceCount == 0 && link.ReferersByLinkerCount == 0 && link.ReferersByTargetCount == 0)
					{
						unusedLinksCounter++;

						if (link.Linker == Net.And)
						{
							Console.WriteLine(link);
						}
					}
				});

			Console.WriteLine("Unused links found: {0}.", unusedLinksCounter);
		}
	}
}
