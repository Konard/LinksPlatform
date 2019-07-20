using System;
using System.Diagnostics;
using System.IO;
using Platform.Examples;
using Platform.Tests;
using Platform.Tests.Collections;
using Platform.Tests.Data.Core;

namespace Platform.Sandbox
{
    public unsafe class Program
    {
        public static void Main(string[] args)
        {
            //StringTests.CapitalizeFirstLetterTest();

            ComparisonTests.GreaterOrEqualPerfomanceTest();

            //args = new string[] { "C:\\compressed.links", "C:\\compressed1.csv", "True", "True", "True" };

            //new CSVExporterCLI<CSVExporter>().Run(args);

            //args = new string[] { "C:\\compressed.links", "C:\\compressed2.csv", "True", "True", "True" };

            //new CSVExporterCLI<CSVSequencesExporter>().Run(args);

            //AllRepeatingSubstringsInString.Run();

            //ReadSequenceTests.ReadSequenceTest();

            //SequencesTests.CompressionEfficiencyTest();

            //new Zadacha().RunAll();

            //new XUnitTestsRunnerCLI().Run(args);

            //new GEXFCSVExporterCLI().Run(args);

            //new CSVExporterCLI().Run(args);
            //new FileIndexerCLI().Run(args);
            //new WikipediaImporterCLI().Run(args);
            //new WikipediaPagesCounterCLI().Run(args);

            //DllImportTest.Test();

            //CompressionExperiments.Test();

            return;

            //Sequences.TestSimplify();
            //WrongPointerTest();
            //UDPTest();
            //Links();

            var sw = Stopwatch.StartNew();

            //Link.StartMemoryManager(@"data.dat");

            Console.WriteLine($"Start time: {sw.Elapsed}");
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

            //Console.WriteLine(Net.Link.Target.ToIndex());

            //Transactions.Run();

            try
            {
                //TerminalExperiment.Run();

                //OperationsExperiments.RunExperiment();
            }
            catch (Exception ex)
            {
            }


            //NewMemoryManagerExperiments.RunExperiment2();

            //NetParserTest.Run();

            Console.WriteLine();

            //Link.StopMemoryManager();

            Console.WriteLine($"Total time: {sw.Elapsed}");
            Console.WriteLine();

            Console.WriteLine("All jobs done.");
            Console.ReadKey();
        }

        static void DisposableNullTest()
        {
      

            Console.WriteLine("ok");
        }

        //static void CompressionDifferenceTests()
        //{
        //    string path = Console.ReadLine();
        //    XmlGenerator.ToFile(path);
        //}

        //static void UnusedLinksTest()
        //{
        //    int unusedLinksCounter = 0;

        //    Link.WalkThroughAllLinks((link) =>
        //    {
        //        if (link.ReferersBySourceCount == 0 && link.ReferersByLinkerCount == 0 && link.ReferersByTargetCount == 0)
        //        {
        //            unusedLinksCounter++;

        //            if (link.Linker == Net.And)
        //            {
        //                Console.WriteLine(link);
        //            }
        //        }
        //    });

        //    Console.WriteLine("Unused links found: {0}.", unusedLinksCounter);
        //}

        private static void WrongPointerTest()
        {
            try
            {
                var array = new byte[256];

                fixed (byte* pointer = array)
                {
                    var value = *(pointer - 259);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private static void Links()
        {
            var filename = (new Random(2)).Next(1, 99999).ToString();

            //if (File.Exists(filename))
            //    File.Delete(filename);

            //ConceptTest.TestGexf(filename);

            if (File.Exists(filename))
                File.Delete(filename);

            Console.ReadKey();
        }
    }
}