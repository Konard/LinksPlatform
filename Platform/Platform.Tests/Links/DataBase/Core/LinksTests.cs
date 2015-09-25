using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Platform.Links.DataBase.CoreUnsafe.Pairs;
using Platform.Links.System.Helpers;

namespace Platform.Tests.Links.DataBase.Core
{
    [TestClass]
    public class LinksTests
    {
        private const long Iterations = 1*1024*1024;

        private static readonly long DefaultLinksSize = (long) Platform.Links.DataBase.CoreUnsafe.Pairs.Links.LinkSizeInBytes*
                                                        1*1024*1024;

        private static readonly Random Rnd = new Random();

        #region Concept

        [TestMethod]
        public void BasicMemoryTest()
        {
            string tempFilename = Path.GetTempFileName();

            using (var links = new Platform.Links.DataBase.CoreUnsafe.Pairs.Links(tempFilename, 1024*1024))
            {
                links.TestBasicMemoryManagement();
            }

            File.Delete(tempFilename);
        }

        #endregion

        #region Performance

        /*
       public static void RunAllPerformanceTests()
       {
           try
           {
               links.TestLinksInSteps();
           }
           catch (Exception ex)
           {
               ex.WriteToConsole();
           }

           return;

           try
           {
               //ThreadPool.SetMaxThreads(2, 2);

               // Запускаем все тесты дважды, чтобы первоначальная инициализация не повлияла на результат
               // Также это дополнительно помогает в отладке
               // Увеличивает вероятность попадания информации в кэши
               for (var i = 0; i < 10; i++)
               {
                   //0 - 10 ГБ
                   //Каждые 100 МБ срез цифр

                   //links.TestGetSourceFunction();
                   //links.TestGetSourceFunctionInParallel();
                   //links.TestGetTargetFunction();
                   //links.TestGetTargetFunctionInParallel();
                   links.Create64BillionLinks();

                   links.TestRandomSearchFixed();
                   //links.Create64BillionLinksInParallel();
                   links.TestEachFunction();
                   //links.TestForeach();
                   //links.TestParallelForeach();
               }

               links.TestDeletionOfAllLinks();

           }
           catch (Exception ex)
           {
               ex.WriteToConsole();
           }
       }*/

        /*
       public static void TestLinksInSteps()
       {
           const long gibibyte = 1024 * 1024 * 1024;
           const long mebibyte = 1024 * 1024;

           var totalLinksToCreate = gibibyte / Platform.Links.DataBase.Core.Pairs.Links.LinkSizeInBytes;
           var linksStep = 102 * mebibyte / Platform.Links.DataBase.Core.Pairs.Links.LinkSizeInBytes;

           var creationMeasurements = new List<TimeSpan>();
           var searchMeasuremets = new List<TimeSpan>();
           var deletionMeasurements = new List<TimeSpan>();

           GetBaseRandomLoopOverhead(linksStep);
           GetBaseRandomLoopOverhead(linksStep);

           var stepLoopOverhead = GetBaseRandomLoopOverhead(linksStep);

           Console.WriteLine("Step loop overhead: {0}.", stepLoopOverhead);

           var loops = totalLinksToCreate / linksStep;

           for (int i = 0; i < loops; i++)
           {
               creationMeasurements.Add(Measure(() => links.RunRandomCreations(linksStep)));
               searchMeasuremets.Add(Measure(() => links.RunRandomSearches(linksStep)));

               Console.Write("\rC + S {0}/{1}", i + 1, loops);
           }

           Console.WriteLine();

           for (int i = 0; i < loops; i++)
           {
               deletionMeasurements.Add(Measure(() => links.RunRandomDeletions(linksStep)));

               Console.Write("\rD {0}/{1}", i + 1, loops);
           }

           Console.WriteLine();

           Console.WriteLine("C S D");

           for (int i = 0; i < loops; i++)
           {
               Console.WriteLine("{0} {1} {2}", creationMeasurements[i], searchMeasuremets[i], deletionMeasurements[i]);
           }

           Console.WriteLine("C S D (no overhead)");

           for (int i = 0; i < loops; i++)
           {
               Console.WriteLine("{0} {1} {2}", creationMeasurements[i] - stepLoopOverhead, searchMeasuremets[i] - stepLoopOverhead, deletionMeasurements[i] - stepLoopOverhead);
           }

           Console.WriteLine("All tests done. Total links left in database: {0}.", links.Total);
       }

       private static void CreatePoints(this Platform.Links.DataBase.Core.Pairs.Links links, long amountToCreate)
       {
           for (long i = 0; i < amountToCreate; i++)
               links.Create(0, 0);
       }
       
        private static TimeSpan GetBaseRandomLoopOverhead(long loops)
        {
            return Measure(() =>
            {
                ulong maxValue = Rnd.NextUInt64();
                ulong result = 0;
                for (long i = 0; i < loops; i++)
                {
                    var source = Rnd.NextUInt64(maxValue);
                    var target = Rnd.NextUInt64(maxValue);

                    result += maxValue + source + target;
                }
                Global.Trash = result;
            });
        }
         */

        [TestMethod]
        public void GetSourceTest()
        {
            string tempFilename = Path.GetTempFileName();

            using (var links = new Platform.Links.DataBase.CoreUnsafe.Pairs.Links(tempFilename, DefaultLinksSize))
            {
                Console.WriteLine("Testing GetSource function with {0} Iterations.", Iterations);

                ulong counter = 0;

                //var firstLink = links.First();
                // Создаём одну связь, из которой будет производить считывание
                ulong firstLink = links.Create(0, 0);

                Stopwatch sw = Stopwatch.StartNew();

                // Тестируем саму функцию
                for (ulong i = 0; i < Iterations; i++)
                    counter += links.GetSource(firstLink);

                TimeSpan elapsedTime = sw.Elapsed;

                double iterationsPerSecond = Iterations/elapsedTime.TotalSeconds;

                // Удаляем связь, из которой производилось считывание
                links.Delete(ref firstLink);

                Console.WriteLine(
                    "{0} Iterations of GetSource function done in {1} ({2} Iterations per second), counter result: {3}",
                    Iterations, elapsedTime, (long) iterationsPerSecond, counter);
            }

            File.Delete(tempFilename);
        }

        [TestMethod]
        public void GetSourceInParallel()
        {
            string tempFilename = Path.GetTempFileName();

            using (var links = new Platform.Links.DataBase.CoreUnsafe.Pairs.Links(tempFilename, DefaultLinksSize))
            {
                Console.WriteLine("Testing GetSource function with {0} Iterations in parallel.", Iterations);

                long counter = 0;

                //var firstLink = links.First();
                ulong firstLink = links.Create(0, 0);

                Stopwatch sw = Stopwatch.StartNew();

                // Тестируем саму функцию
                Parallel.For(0, Iterations, x =>
                {
                    Interlocked.Add(ref counter, (long) links.GetSource(firstLink));
                    //Interlocked.Increment(ref counter);
                });

                TimeSpan elapsedTime = sw.Elapsed;

                double iterationsPerSecond = Iterations/elapsedTime.TotalSeconds;

                links.Delete(ref firstLink);

                Console.WriteLine(
                    "{0} Iterations of GetSource function done in {1} ({2} Iterations per second), counter result: {3}",
                    Iterations, elapsedTime, (long) iterationsPerSecond, counter);
            }

            File.Delete(tempFilename);
        }

        [TestMethod]
        public void TestGetTarget()
        {
            string tempFilename = Path.GetTempFileName();

            using (var links = new Platform.Links.DataBase.CoreUnsafe.Pairs.Links(tempFilename, DefaultLinksSize))
            {
                Console.WriteLine("Testing GetTarget function with {0} Iterations.", Iterations);

                ulong counter = 0;

                //var firstLink = links.First();
                ulong firstLink = links.Create(0, 0);

                Stopwatch sw = Stopwatch.StartNew();

                for (ulong i = 0; i < Iterations; i++)
                    counter += links.GetTarget(firstLink);

                TimeSpan elapsedTime = sw.Elapsed;

                double iterationsPerSecond = Iterations/elapsedTime.TotalSeconds;

                links.Delete(ref firstLink);

                Console.WriteLine(
                    "{0} Iterations of GetTarget function done in {1} ({2} Iterations per second), counter result: {3}",
                    Iterations, elapsedTime, (long) iterationsPerSecond, counter);
            }

            File.Delete(tempFilename);
        }

        [TestMethod]
        public void TestGetTargetInParallel()
        {
            string tempFilename = Path.GetTempFileName();

            using (var links = new Platform.Links.DataBase.CoreUnsafe.Pairs.Links(tempFilename, DefaultLinksSize))
            {
                Console.WriteLine("Testing GetTarget function with {0} Iterations in parallel.", Iterations);

                long counter = 0;

                //var firstLink = links.First();
                ulong firstLink = links.Create(0, 0);

                Stopwatch sw = Stopwatch.StartNew();

                Parallel.For(0, Iterations, x =>
                {
                    Interlocked.Add(ref counter, (long) links.GetTarget(firstLink));
                    //Interlocked.Increment(ref counter);
                });

                TimeSpan elapsedTime = sw.Elapsed;

                double iterationsPerSecond = Iterations/elapsedTime.TotalSeconds;

                links.Delete(ref firstLink);

                Console.WriteLine(
                    "{0} Iterations of GetTarget function done in {1} ({2} Iterations per second), counter result: {3}",
                    Iterations, elapsedTime, (long) iterationsPerSecond, counter);
            }

            File.Delete(tempFilename);
        }

        // TODO: Заполнить базу данных перед тестом
        /*
        [TestMethod]
        public void TestRandomSearchFixed()
        {
            var tempFilename = Path.GetTempFileName();

            using (var links = new Platform.Links.DataBase.Core.Pairs.Links(tempFilename, DefaultLinksSize))
            {
                long iterations = 64 * 1024 * 1024 / Platform.Links.DataBase.Core.Pairs.Links.LinkSizeInBytes;

                ulong counter = 0;

                var minLink = 1UL;
                var maxLink = links.Total;

                var rnd = new Random((int)DateTime.UtcNow.Ticks);

                Console.WriteLine("Testing Random Search with {0} Iterations.", iterations);

                var sw = Stopwatch.StartNew();

                for (var i = iterations; i > 0; i--)
                {
                    var source = rnd.NextUInt64(minLink, maxLink);
                    var target = rnd.NextUInt64(minLink, maxLink);

                    counter += links.Search(source, target);
                }

                var elapsedTime = sw.Elapsed;

                var iterationsPerSecond = iterations / elapsedTime.TotalSeconds;

                Console.WriteLine("{0} Iterations of Random Search done in {1} ({2} Iterations per second), c: {3}", iterations, elapsedTime, (long)iterationsPerSecond, counter);
            }

            File.Delete(tempFilename);
        }*/

        [TestMethod]
        public void TestRandomSearchAll()
        {
            string tempFilename = Path.GetTempFileName();

            using (var links = new Platform.Links.DataBase.CoreUnsafe.Pairs.Links(tempFilename, DefaultLinksSize))
            {
                ulong counter = 0;

                ulong minLink = 1UL;
                ulong maxLink = links.Total;

                ulong iterations = links.Total;
                var rnd = new Random((int) DateTime.UtcNow.Ticks);

                Console.WriteLine("Testing Random Search with {0} Iterations.", links.Total);

                Stopwatch sw = Stopwatch.StartNew();

                for (ulong i = iterations; i > 0; i--)
                {
                    ulong source = rnd.NextUInt64(minLink, maxLink);
                    ulong target = rnd.NextUInt64(minLink, maxLink);

                    counter += links.Search(source, target);
                }

                TimeSpan elapsedTime = sw.Elapsed;

                double iterationsPerSecond = iterations/elapsedTime.TotalSeconds;

                Console.WriteLine("{0} Iterations of Random Search done in {1} ({2} Iterations per second), c: {3}",
                    iterations, elapsedTime, (long) iterationsPerSecond, counter);
            }

            File.Delete(tempFilename);
        }

        [TestMethod]
        public void TestEach()
        {
            string tempFilename = Path.GetTempFileName();

            using (var links = new Platform.Links.DataBase.CoreUnsafe.Pairs.Links(tempFilename, DefaultLinksSize))
            {
                ulong counter = 0;

                Console.WriteLine("Testing Each function.");

                Stopwatch sw = Stopwatch.StartNew();

                links.Each(0, 0, x =>
                {
                    counter++;

                    return true;
                });

                TimeSpan elapsedTime = sw.Elapsed;

                double linksPerSecond = counter/elapsedTime.TotalSeconds;

                Console.WriteLine("{0} Iterations of Each's handler function done in {1} ({2} links per second)",
                    counter, elapsedTime, (long) linksPerSecond);
            }

            File.Delete(tempFilename);
        }

        /*
        [TestMethod]
        public static void TestForeach()
        {
            var tempFilename = Path.GetTempFileName();

            using (var links = new Platform.Links.DataBase.Core.Pairs.Links(tempFilename, DefaultLinksSize))
            {
                ulong counter = 0;

                Console.WriteLine("Testing foreach through links.");

                var sw = Stopwatch.StartNew();

                //foreach (var link in links)
                //{
                //    counter++;
                //}

                var elapsedTime = sw.Elapsed;

                var linksPerSecond = (double)counter / elapsedTime.TotalSeconds;

                Console.WriteLine("{0} Iterations of Foreach's handler block done in {1} ({2} links per second)", counter, elapsedTime, (long)linksPerSecond);
            }

            File.Delete(tempFilename);
        }
        */

        /*
        [TestMethod]
        public static void TestParallelForeach()
        {
            var tempFilename = Path.GetTempFileName();

            using (var links = new Platform.Links.DataBase.Core.Pairs.Links(tempFilename, DefaultLinksSize))
            {

                long counter = 0;

                Console.WriteLine("Testing parallel foreach through links.");

                var sw = Stopwatch.StartNew();

                //Parallel.ForEach((IEnumerable<ulong>)links, x =>
                //{
                //    Interlocked.Increment(ref counter);
                //});

                var elapsedTime = sw.Elapsed;

                var linksPerSecond = (double)counter / elapsedTime.TotalSeconds;

                Console.WriteLine("{0} Iterations of Parallel Foreach's handler block done in {1} ({2} links per second)", counter, elapsedTime, (long)linksPerSecond);
            }

            File.Delete(tempFilename);
        }
        */

        [TestMethod]
        public static void Create64BillionLinks()
        {
            string tempFilename = Path.GetTempFileName();

            using (var links = new Platform.Links.DataBase.CoreUnsafe.Pairs.Links(tempFilename, DefaultLinksSize))
            {
                ulong linksBeforeTest = links.Total;

                long linksToCreate = 64*1024*1024/Platform.Links.DataBase.CoreUnsafe.Pairs.Links.LinkSizeInBytes;

                Console.WriteLine("Creating {0} links.", linksToCreate);

                TimeSpan elapsedTime = Measure(() =>
                {
                    for (long i = 0; i < linksToCreate; i++)
                    {
                        links.Create(0, 0);
                    }
                });

                ulong linksCreated = links.Total - linksBeforeTest;
                double linksPerSecond = linksCreated/elapsedTime.TotalSeconds;

                Console.WriteLine("Current links count: {0}.", links.Total);

                Console.WriteLine("{0} links created in {1} ({2} links per second)", linksCreated, elapsedTime,
                    (long) linksPerSecond);
            }

            File.Delete(tempFilename);
        }

        [TestMethod]
        public static void Create64BillionLinksInParallel()
        {
            string tempFilename = Path.GetTempFileName();

            using (var links = new Platform.Links.DataBase.CoreUnsafe.Pairs.Links(tempFilename, DefaultLinksSize))
            {
                ulong linksBeforeTest = links.Total;

                Stopwatch sw = Stopwatch.StartNew();

                long linksToCreate = 64*1024*1024/Platform.Links.DataBase.CoreUnsafe.Pairs.Links.LinkSizeInBytes;

                Console.WriteLine("Creating {0} links in parallel.", linksToCreate);

                Parallel.For(0, linksToCreate, x => links.Create(0, 0));

                TimeSpan elapsedTime = sw.Elapsed;

                ulong linksCreated = links.Total - linksBeforeTest;
                double linksPerSecond = linksCreated/elapsedTime.TotalSeconds;

                Console.WriteLine("{0} links created in {1} ({2} links per second)", linksCreated, elapsedTime,
                    (long) linksPerSecond);
            }

            File.Delete(tempFilename);
        }

        [TestMethod]
        public static void TestDeletionOfAllLinks()
        {
            string tempFilename = Path.GetTempFileName();

            using (var links = new Platform.Links.DataBase.CoreUnsafe.Pairs.Links(tempFilename, DefaultLinksSize))
            {
                ulong linksBeforeTest = links.Total;

                Console.WriteLine("Deleting all links");

                TimeSpan elapsedTime = Measure(links.DeleteAllLinks);

                ulong linksDeleted = linksBeforeTest - links.Total;
                double linksPerSecond = linksDeleted/elapsedTime.TotalSeconds;

                Console.WriteLine("{0} links deleted in {1} ({2} links per second)", linksDeleted, elapsedTime,
                    (long) linksPerSecond);
            }

            File.Delete(tempFilename);
        }

        public static TimeSpan Measure(Action action)
        {
            Stopwatch sw = Stopwatch.StartNew();

            action();

            sw.Stop();
            return sw.Elapsed;
        }

        #endregion
    }
}