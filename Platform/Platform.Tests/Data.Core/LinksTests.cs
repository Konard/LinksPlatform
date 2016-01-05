using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Platform.Data.Core.Pairs;
using Platform.Helpers;

namespace Platform.Tests.Data.Core
{
    [TestClass]
    public class LinksTests
    {
        private const long Iterations = 10 * 1024;

        private static readonly long DefaultLinksSizeStep = LinksMemoryManager.LinkSizeInBytes * 1024 * 1024;

        #region Concept

        [TestMethod]
        public void CascadeUpdateTest()
        {
            var tempDatabaseFilename = Path.GetTempFileName();
            var tempTransactionLogFilename = Path.GetTempFileName();

            const ulong itself = LinksConstants.Itself;

            using (var memoryManager = new LinksMemoryManager(tempDatabaseFilename, DefaultLinksSizeStep))
            using (var links = new Links(memoryManager, tempTransactionLogFilename))
            {
                var l1 = links.Create(itself, itself);
                var l2 = links.Create(itself, itself);

                l2 = links.Update(l2, l2, l1, l2);

                links.Create(l2, itself);
                links.Create(l2, itself);

                l2 = links.Update(l2, l1);

                links.Delete(l2);

                Global.Trash = links.Count();
            }

            Global.Trash = FileHelpers
                .ReadAll<Links.Transition>(tempTransactionLogFilename);

            File.Delete(tempDatabaseFilename);
            File.Delete(tempTransactionLogFilename);
        }

        [TestMethod]
        public void BasicTransactionLogTest()
        {
            var tempDatabaseFilename = Path.GetTempFileName();
            var tempTransactionLogFilename = Path.GetTempFileName();

            const ulong itself = LinksConstants.Itself;

            using (var memoryManager = new LinksMemoryManager(tempDatabaseFilename, DefaultLinksSizeStep))
            using (var links = new Links(memoryManager, tempTransactionLogFilename))
            {
                var l1 = links.Create(itself, itself);
                var l2 = links.Create(itself, itself);

                Global.Trash = links.Update(l2, l2, l1, l2);

                links.Delete(l1);
            }

            Global.Trash = FileHelpers
                .ReadAll<Links.Transition>(tempTransactionLogFilename);

            File.Delete(tempDatabaseFilename);
            File.Delete(tempTransactionLogFilename);
        }

        [TestMethod]
        public void TransactionsTest()
        {
            var tempDatabaseFilename = Path.GetTempFileName();
            var tempTransactionLogFilename = Path.GetTempFileName();

            const ulong itself = LinksConstants.Itself;

            // Auto Reverted (Because no commit at transaction)
            using (var memoryManager = new LinksMemoryManager(tempDatabaseFilename, DefaultLinksSizeStep))
            using (var links = new Links(memoryManager, tempTransactionLogFilename))
            {
                using (var transaction = links.BeginTransaction())
                {
                    var l1 = links.Create(itself, itself);
                    var l2 = links.Create(itself, itself);

                    Global.Trash = links.Update(l2, l2, l1, l2);

                    links.Delete(l1);

                    Global.Trash = transaction;
                }

                Global.Trash = links.Count();
            }

            Global.Trash = FileHelpers
                .ReadAll<Links.Transition>(tempTransactionLogFilename);

            // User Code Error (Autoreverted), no data saved
            try
            {
                using (var memoryManager = new LinksMemoryManager(tempDatabaseFilename, DefaultLinksSizeStep))
                using (var links = new Links(memoryManager, tempTransactionLogFilename))
                {
                    using (var transaction = links.BeginTransaction())
                    {
                        var l1 = links.Create(itself, itself);
                        var l2 = links.Create(itself, itself);

                        l2 = links.Update(l2, l2, l1, l2);

                        links.Create(l2, itself);
                        links.Create(l2, itself);

                        Global.Trash = FileHelpers.ReadAll<Links.Transition>(tempTransactionLogFilename);

                        l2 = links.Update(l2, l1);

                        links.Delete(l2);

                        ExceptionThrower();

                        transaction.Commit();
                    }

                    Global.Trash = links.Count();
                }
            }
            catch
            {
                var transitions = FileHelpers
                    .ReadAll<Links.Transition>(tempTransactionLogFilename);

                Assert.IsTrue(transitions.Length == 1 && transitions[0].Before.IsNull() && transitions[0].After.IsNull());
            }

            // User Code Error (Autoreverted), some data saved
            try
            {
                ulong l1;
                ulong l2;

                using (var memoryManager = new LinksMemoryManager(tempDatabaseFilename, DefaultLinksSizeStep))
                using (var links = new Links(memoryManager, tempTransactionLogFilename))
                {
                    l1 = links.Create(itself, itself);
                    l2 = links.Create(itself, itself);

                    l2 = links.Update(l2, l2, l1, l2);

                    links.Create(l2, itself);
                    links.Create(l2, itself);
                }

                Global.Trash = FileHelpers.ReadAll<Links.Transition>(tempTransactionLogFilename);

                using (var memoryManager = new LinksMemoryManager(tempDatabaseFilename, DefaultLinksSizeStep))
                using (var links = new Links(memoryManager, tempTransactionLogFilename))
                {
                    using (var transaction = links.BeginTransaction())
                    {
                        l2 = links.Update(l2, l1);

                        links.Delete(l2);

                        ExceptionThrower();

                        transaction.Commit();
                    }

                    Global.Trash = links.Count();
                }
            }
            catch
            {
                Global.Trash = FileHelpers
                    .ReadAll<Links.Transition>(tempTransactionLogFilename);
            }

            // Commit
            using (var memoryManager = new LinksMemoryManager(tempDatabaseFilename, DefaultLinksSizeStep))
            using (var links = new Links(memoryManager, tempTransactionLogFilename))
            {
                using (var transaction = links.BeginTransaction())
                {
                    var l1 = links.Create(itself, itself);
                    var l2 = links.Create(itself, itself);

                    Global.Trash = links.Update(l2, l2, l1, l2);

                    links.Delete(l1);

                    transaction.Commit();
                }

                Global.Trash = links.Count();
            }

            Global.Trash = FileHelpers
                .ReadAll<Links.Transition>(tempTransactionLogFilename);

            // Damage database

            FileHelpers
                .WriteFirst(tempTransactionLogFilename, new Links.Transition { TransactionId = 555 });

            // Try load damaged database
            try
            {
                // TODO: Fix
                using (var memoryManager = new LinksMemoryManager(tempDatabaseFilename, DefaultLinksSizeStep))
                using (var links = new Links(memoryManager, tempTransactionLogFilename))
                {
                    Global.Trash = links.Count();
                }
            }
            catch (NotSupportedException ex)
            {
                Assert.IsTrue(ex.Message == "Database is damaged, autorecovery is not supported yet.");
            }

            Global.Trash = FileHelpers
                .ReadAll<Links.Transition>(tempTransactionLogFilename);

            File.Delete(tempDatabaseFilename);
            File.Delete(tempTransactionLogFilename);
        }

        [TestMethod]
        public void Bug1Test()
        {
            var tempDatabaseFilename = Path.GetTempFileName();
            var tempTransactionLogFilename = Path.GetTempFileName();

            const ulong itself = LinksConstants.Itself;

            // User Code Error (Autoreverted), some data saved
            try
            {
                ulong l1;
                ulong l2;

                using (var memoryManager = new LinksMemoryManager(tempDatabaseFilename, DefaultLinksSizeStep))
                using (var links = new Links(memoryManager, tempTransactionLogFilename))
                {
                    l1 = links.Create(itself, itself);
                    l2 = links.Create(itself, itself);

                    l2 = links.Update(l2, l2, l1, l2);

                    links.Create(l2, itself);
                    links.Create(l2, itself);
                }

                Global.Trash = FileHelpers.ReadAll<Links.Transition>(tempTransactionLogFilename);

                using (var memoryManager = new LinksMemoryManager(tempDatabaseFilename, DefaultLinksSizeStep))
                using (var links = new Links(memoryManager, tempTransactionLogFilename))
                {
                    using (var transaction = links.BeginTransaction())
                    {
                        l2 = links.Update(l2, l1);

                        links.Delete(l2);

                        ExceptionThrower();

                        transaction.Commit();
                    }

                    Global.Trash = links.Count();
                }
            }
            catch
            {
                Global.Trash = FileHelpers
                    .ReadAll<Links.Transition>(tempTransactionLogFilename);
            }

            File.Delete(tempDatabaseFilename);
            File.Delete(tempTransactionLogFilename);
        }

        public void ExceptionThrower()
        {
            throw new Exception();
        }

        [TestMethod]
        public void PathsTest()
        {
            var tempDatabaseFilename = Path.GetTempFileName();
            var tempTransactionLogFilename = Path.GetTempFileName();

            const ulong itself = LinksConstants.Itself;
            const long source = LinksConstants.SourcePart;
            const long target = LinksConstants.TargetPart;

            using (var memoryManager = new LinksMemoryManager(tempDatabaseFilename, DefaultLinksSizeStep))
            using (var links = new Links(memoryManager, tempTransactionLogFilename))
            {
                var l1 = links.Create(itself, itself);
                var l2 = links.Create(itself, itself);

                var r1 = links.GetByKeys(l1, source, target, source);
                var r2 = links.CheckPathExistance(l2, l2, l2, l2);
            }

            File.Delete(tempDatabaseFilename);
            File.Delete(tempTransactionLogFilename);
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
                ulong maxValue = RandomHelpers.DefaultFactory.NextUInt64();
                ulong result = 0;
                for (long i = 0; i < loops; i++)
                {
                    var source = RandomHelpers.DefaultFactory.NextUInt64(maxValue);
                    var target = RandomHelpers.DefaultFactory.NextUInt64(maxValue);

                    result += maxValue + source + target;
                }
                Global.Trash = result;
            });
        }
         */

        [TestMethod]
        public void GetSourceTest()
        {
            var tempFilename = Path.GetTempFileName();

            using (var memoryManager = new LinksMemoryManager(tempFilename, DefaultLinksSizeStep))
            using (var links = new Links(memoryManager))
            {
                Console.WriteLine("Testing GetSource function with {0} Iterations.", Iterations);

                ulong counter = 0;

                //var firstLink = links.First();
                // Создаём одну связь, из которой будет производить считывание
                var firstLink = links.Create(0, 0);

                var sw = Stopwatch.StartNew();

                // Тестируем саму функцию
                for (ulong i = 0; i < Iterations; i++)
                    counter += links.GetSource(firstLink);

                var elapsedTime = sw.Elapsed;

                var iterationsPerSecond = Iterations / elapsedTime.TotalSeconds;

                // Удаляем связь, из которой производилось считывание
                links.Delete(firstLink);

                Console.WriteLine(
                    "{0} Iterations of GetSource function done in {1} ({2} Iterations per second), counter result: {3}",
                    Iterations, elapsedTime, (long)iterationsPerSecond, counter);
            }

            File.Delete(tempFilename);
        }

        [TestMethod]
        public void GetSourceInParallel()
        {
            var tempFilename = Path.GetTempFileName();

            using (var memoryManager = new LinksMemoryManager(tempFilename, DefaultLinksSizeStep))
            using (var links = new Links(memoryManager))
            {
                Console.WriteLine("Testing GetSource function with {0} Iterations in parallel.", Iterations);

                long counter = 0;

                //var firstLink = links.First();
                var firstLink = links.Create(0, 0);

                var sw = Stopwatch.StartNew();

                // Тестируем саму функцию
                Parallel.For(0, Iterations, x =>
                {
                    Interlocked.Add(ref counter, (long)links.GetSource(firstLink));
                    //Interlocked.Increment(ref counter);
                });

                var elapsedTime = sw.Elapsed;

                var iterationsPerSecond = Iterations / elapsedTime.TotalSeconds;

                links.Delete(firstLink);

                Console.WriteLine(
                    "{0} Iterations of GetSource function done in {1} ({2} Iterations per second), counter result: {3}",
                    Iterations, elapsedTime, (long)iterationsPerSecond, counter);
            }

            File.Delete(tempFilename);
        }

        [TestMethod]
        public void TestGetTarget()
        {
            var tempFilename = Path.GetTempFileName();

            using (var memoryManager = new LinksMemoryManager(tempFilename, DefaultLinksSizeStep))
            using (var links = new Links(memoryManager))
            {
                Console.WriteLine("Testing GetTarget function with {0} Iterations.", Iterations);

                ulong counter = 0;

                //var firstLink = links.First();
                var firstLink = links.Create(0, 0);

                var sw = Stopwatch.StartNew();

                for (ulong i = 0; i < Iterations; i++)
                    counter += links.GetTarget(firstLink);

                var elapsedTime = sw.Elapsed;

                var iterationsPerSecond = Iterations / elapsedTime.TotalSeconds;

                links.Delete(firstLink);

                Console.WriteLine(
                    "{0} Iterations of GetTarget function done in {1} ({2} Iterations per second), counter result: {3}",
                    Iterations, elapsedTime, (long)iterationsPerSecond, counter);
            }

            File.Delete(tempFilename);
        }

        [TestMethod]
        public void TestGetTargetInParallel()
        {
            var tempFilename = Path.GetTempFileName();

            using (var memoryManager = new LinksMemoryManager(tempFilename, DefaultLinksSizeStep))
            using (var links = new Links(memoryManager))
            {
                Console.WriteLine("Testing GetTarget function with {0} Iterations in parallel.", Iterations);

                long counter = 0;

                //var firstLink = links.First();
                var firstLink = links.Create(0, 0);

                var sw = Stopwatch.StartNew();

                Parallel.For(0, Iterations, x =>
                {
                    Interlocked.Add(ref counter, (long)links.GetTarget(firstLink));
                    //Interlocked.Increment(ref counter);
                });

                var elapsedTime = sw.Elapsed;

                var iterationsPerSecond = Iterations / elapsedTime.TotalSeconds;

                links.Delete(firstLink);

                Console.WriteLine(
                    "{0} Iterations of GetTarget function done in {1} ({2} Iterations per second), counter result: {3}",
                    Iterations, elapsedTime, (long)iterationsPerSecond, counter);
            }

            File.Delete(tempFilename);
        }

        // TODO: Заполнить базу данных перед тестом
        /*
        [TestMethod]
        public void TestRandomSearchFixed()
        {
            var tempFilename = Path.GetTempFileName();

            using (var links = new Platform.Links.DataBase.Core.Pairs.Links(tempFilename, DefaultLinksSizeStep))
            {
                long iterations = 64 * 1024 * 1024 / Platform.Links.DataBase.Core.Pairs.Links.LinkSizeInBytes;

                ulong counter = 0;
                var maxLink = links.Total;

                Console.WriteLine("Testing Random Search with {0} Iterations.", iterations);

                var sw = Stopwatch.StartNew();

                for (var i = iterations; i > 0; i--)
                {
                    var source = RandomHelpers.DefaultFactory.NextUInt64(LinksConstants.MinPossibleIndex, maxLink);
                    var target = RandomHelpers.DefaultFactory.NextUInt64(LinksConstants.MinPossibleIndex, maxLink);

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
            var tempFilename = Path.GetTempFileName();

            using (var memoryManager = new LinksMemoryManager(tempFilename, DefaultLinksSizeStep))
            using (var links = new Links(memoryManager))
            {
                ulong counter = 0;

                var maxLink = links.Count();

                var iterations = links.Count();

                Console.WriteLine("Testing Random Search with {0} Iterations.", links.Count());

                var sw = Stopwatch.StartNew();

                for (var i = iterations; i > 0; i--)
                {
                    var source = RandomHelpers.DefaultFactory.NextUInt64(LinksConstants.MinPossibleIndex, maxLink);
                    var target = RandomHelpers.DefaultFactory.NextUInt64(LinksConstants.MinPossibleIndex, maxLink);

                    counter += links.Search(source, target);
                }

                var elapsedTime = sw.Elapsed;

                var iterationsPerSecond = iterations / elapsedTime.TotalSeconds;

                Console.WriteLine("{0} Iterations of Random Search done in {1} ({2} Iterations per second), c: {3}",
                    iterations, elapsedTime, (long)iterationsPerSecond, counter);
            }

            File.Delete(tempFilename);
        }

        [TestMethod]
        public void TestEach()
        {
            var tempFilename = Path.GetTempFileName();

            using (var memoryManager = new LinksMemoryManager(tempFilename, DefaultLinksSizeStep))
            using (var links = new Links(memoryManager))
            {
                ulong counter = 0;

                Console.WriteLine("Testing Each function.");

                var sw = Stopwatch.StartNew();

                links.Each(0, 0, x =>
                {
                    counter++;

                    return true;
                });

                var elapsedTime = sw.Elapsed;

                var linksPerSecond = counter / elapsedTime.TotalSeconds;

                Console.WriteLine("{0} Iterations of Each's handler function done in {1} ({2} links per second)",
                    counter, elapsedTime, (long)linksPerSecond);
            }

            File.Delete(tempFilename);
        }

        /*
        [TestMethod]
        public static void TestForeach()
        {
            var tempFilename = Path.GetTempFileName();

            using (var links = new Platform.Links.DataBase.Core.Pairs.Links(tempFilename, DefaultLinksSizeStep))
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

            using (var links = new Platform.Links.DataBase.Core.Pairs.Links(tempFilename, DefaultLinksSizeStep))
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
            var tempFilename = Path.GetTempFileName();

            using (var memoryManager = new LinksMemoryManager(tempFilename, DefaultLinksSizeStep))
            using (var links = new Links(memoryManager))
            {
                var linksBeforeTest = links.Count();

                long linksToCreate = 64 * 1024 * 1024 / LinksMemoryManager.LinkSizeInBytes;

                Console.WriteLine("Creating {0} links.", linksToCreate);

                var elapsedTime = PerformanceHelpers.Measure(() =>
                {
                    for (long i = 0; i < linksToCreate; i++)
                    {
                        links.Create(0, 0);
                    }
                });

                var linksCreated = links.Count() - linksBeforeTest;
                var linksPerSecond = linksCreated / elapsedTime.TotalSeconds;

                Console.WriteLine("Current links count: {0}.", links.Count());

                Console.WriteLine("{0} links created in {1} ({2} links per second)", linksCreated, elapsedTime,
                    (long)linksPerSecond);
            }

            File.Delete(tempFilename);
        }

        [TestMethod]
        public static void Create64BillionLinksInParallel()
        {
            var tempFilename = Path.GetTempFileName();

            using (var memoryManager = new LinksMemoryManager(tempFilename, DefaultLinksSizeStep))
            using (var links = new Links(memoryManager))
            {
                var linksBeforeTest = links.Count();

                var sw = Stopwatch.StartNew();

                long linksToCreate = 64 * 1024 * 1024 / LinksMemoryManager.LinkSizeInBytes;

                Console.WriteLine("Creating {0} links in parallel.", linksToCreate);

                Parallel.For(0, linksToCreate, x => links.Create(0, 0));

                var elapsedTime = sw.Elapsed;

                var linksCreated = links.Count() - linksBeforeTest;
                var linksPerSecond = linksCreated / elapsedTime.TotalSeconds;

                Console.WriteLine("{0} links created in {1} ({2} links per second)", linksCreated, elapsedTime,
                    (long)linksPerSecond);
            }

            File.Delete(tempFilename);
        }

        [TestMethod]
        public static void TestDeletionOfAllLinks()
        {
            var tempFilename = Path.GetTempFileName();

            using (var memoryManager = new LinksMemoryManager(tempFilename, DefaultLinksSizeStep))
            using (var links = new Links(memoryManager))
            {
                var linksBeforeTest = links.Count();

                Console.WriteLine("Deleting all links");

                var elapsedTime = PerformanceHelpers.Measure(links.DeleteAll);

                var linksDeleted = linksBeforeTest - links.Count();
                var linksPerSecond = linksDeleted / elapsedTime.TotalSeconds;

                Console.WriteLine("{0} links deleted in {1} ({2} links per second)", linksDeleted, elapsedTime,
                    (long)linksPerSecond);
            }

            File.Delete(tempFilename);
        }

        #endregion
    }
}