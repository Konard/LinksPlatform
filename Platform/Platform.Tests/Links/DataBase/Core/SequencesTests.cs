using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Platform.Links.DataBase.CoreUnsafe.Sequences;
using Pairs = Platform.Links.DataBase.CoreUnsafe.Pairs;

namespace Platform.Tests.Links.DataBase.Core
{
    [TestClass]
    public class SequencesTests
    {
        [TestMethod]
        public void CreateAllVariantsTest()
        {
            string tempFilename = Path.GetTempFileName();

            const long sequenceLength = 8;

            const ulong itself = Pairs.Links.Itself;

            using (var links = new Pairs.Links(tempFilename, 1024 * 1024))
            {
                var sequence = new ulong[sequenceLength];
                for (int i = 0; i < sequenceLength; i++)
                    sequence[i] = links.Create(itself, itself);

                var sequences = new Sequences(links);

                var sw1 = Stopwatch.StartNew();
                var results1 = sequences.CreateAllVariants1(sequence); sw1.Stop();

                var sw2 = Stopwatch.StartNew();
                var results2 = sequences.CreateAllVariants2(sequence); sw2.Stop();

                Assert.IsTrue(results1.Count > results2.Length);
                Assert.IsTrue(sw1.Elapsed > sw2.Elapsed);

                for (int i = 0; i < sequenceLength; i++)
                    links.Delete(sequence[i]);
            }

            File.Delete(tempFilename);
        }

        [TestMethod]
        public void AllVariantsSearchTest()
        {
            string tempFilename = Path.GetTempFileName();

            const long sequenceLength = 8;

            const ulong itself = Pairs.Links.Itself;

            using (var links = new Pairs.Links(tempFilename, 1024 * 1024))
            {
                var sequence = new ulong[sequenceLength];
                for (int i = 0; i < sequenceLength; i++)
                    sequence[i] = links.Create(itself, itself);

                var sequences = new Sequences(links);

                var createResults = sequences.CreateAllVariants2(sequence).Distinct().ToArray();

                var sw0 = Stopwatch.StartNew();
                var searchResults0 = sequences.GetAllMatchingSequences0(sequence); sw0.Stop();

                var sw1 = Stopwatch.StartNew();
                var searchResults1 = sequences.GetAllMatchingSequences1(sequence); sw1.Stop();

                var sw2 = Stopwatch.StartNew();
                var searchResults2 = sequences.Each(sequence); sw2.Stop();

                var intersection0 = createResults.Intersect(searchResults0).ToList();
                Assert.IsTrue(intersection0.Count == searchResults0.Count);
                Assert.IsTrue(intersection0.Count == createResults.Length);

                var intersection1 = createResults.Intersect(searchResults1).ToList();
                Assert.IsTrue(intersection1.Count == searchResults1.Count);
                Assert.IsTrue(intersection1.Count == createResults.Length);

                var intersection2 = createResults.Intersect(searchResults2).ToList();
                Assert.IsTrue(intersection2.Count == searchResults2.Count);
                Assert.IsTrue(intersection2.Count == createResults.Length);

                Assert.IsTrue(sw1.Elapsed < sw2.Elapsed);

                for (int i = 0; i < sequenceLength; i++)
                    links.Delete(sequence[i]);
            }

            File.Delete(tempFilename);
        }

        [TestMethod]
        public void BalancedVariantSearchTest()
        {
            string tempFilename = Path.GetTempFileName();

            const long sequenceLength = 200;

            const ulong itself = Pairs.Links.Itself;

            using (var links = new Pairs.Links(tempFilename, 1024 * 1024))
            {
                var sequence = new ulong[sequenceLength];
                for (int i = 0; i < sequenceLength; i++)
                    sequence[i] = links.Create(itself, itself);

                var sequences = new Sequences(links);

                var sw1 = Stopwatch.StartNew();
                var balancedVariant = sequences.CreateBalancedVariant(sequence); sw1.Stop();

                var sw2 = Stopwatch.StartNew();
                var searchResults2 = sequences.GetAllMatchingSequences0(sequence); sw2.Stop();

                var sw3 = Stopwatch.StartNew();
                var searchResults3 = sequences.GetAllMatchingSequences1(sequence); sw3.Stop();

                // На количестве в 200 элементов это будет занимать вечность
                //var sw4 = Stopwatch.StartNew();
                //var searchResults4 = sequences.Each(sequence); sw4.Stop();

                Assert.IsTrue(searchResults2.Count == 1 && balancedVariant == searchResults2[0]);

                Assert.IsTrue(searchResults3.Count == 1 && balancedVariant == searchResults3[0]);

                //Assert.IsTrue(sw1.Elapsed < sw2.Elapsed);

                for (int i = 0; i < sequenceLength; i++)
                    links.Delete(sequence[i]);
            }

            File.Delete(tempFilename);
        }

        [TestMethod]
        public void AllPartialVariantsSearchTest()
        {
            string tempFilename = Path.GetTempFileName();

            const long sequenceLength = 8;

            const ulong itself = Pairs.Links.Itself;

            using (var links = new Pairs.Links(tempFilename, 1024 * 1024))
            {
                var sequence = new ulong[sequenceLength];
                for (int i = 0; i < sequenceLength; i++)
                    sequence[i] = links.Create(itself, itself);

                var sequences = new Sequences(links);

                var createResults = sequences.CreateAllVariants2(sequence);

                //var createResultsStrings = createResults.Select(x => x + ": " + sequences.FormatSequence(x)).ToList();
                //Global.Trash = createResultsStrings;

                var partialSequence = new ulong[sequenceLength - 2];

                Array.Copy(sequence, 1, partialSequence, 0, sequenceLength - 2);

                var sw1 = Stopwatch.StartNew();
                var searchResults1 = sequences.GetAllPartiallyMatchingSequences0(partialSequence); sw1.Stop();

                var sw2 = Stopwatch.StartNew();
                var searchResults2 = sequences.GetAllPartiallyMatchingSequences1(partialSequence); sw2.Stop();

                //var sw3 = Stopwatch.StartNew();
                //var searchResults3 = sequences.GetAllPartiallyMatchingSequences2(partialSequence); sw3.Stop();

                //Global.Trash = searchResults3;

                //var searchResults1Strings = searchResults1.Select(x => x + ": " + sequences.FormatSequence(x)).ToList();
                //Global.Trash = searchResults1Strings;

                var intersection1 = createResults.Intersect(searchResults1).ToList();
                Assert.IsTrue(intersection1.Count < searchResults1.Count);
                Assert.IsTrue(intersection1.Count == createResults.Length);

                var intersection2 = createResults.Intersect(searchResults2).ToList();
                Assert.IsTrue(intersection2.Count < searchResults2.Count);
                Assert.IsTrue(intersection2.Count == createResults.Length);

                for (int i = 0; i < sequenceLength; i++)
                    links.Delete(sequence[i]);
            }

            File.Delete(tempFilename);
        }


        [TestMethod]
        public void BalancedPartialVariantsSearchTest()
        {
            string tempFilename = Path.GetTempFileName();

            const long sequenceLength = 200;

            const ulong itself = Pairs.Links.Itself;

            using (var links = new Pairs.Links(tempFilename, 1024 * 1024))
            {
                var sequence = new ulong[sequenceLength];
                for (int i = 0; i < sequenceLength; i++)
                    sequence[i] = links.Create(itself, itself);

                var sequences = new Sequences(links);

                var balancedVariant = sequences.CreateBalancedVariant(sequence);

                var partialSequence = new ulong[sequenceLength - 2];

                Array.Copy(sequence, 1, partialSequence, 0, sequenceLength - 2);

                var sw1 = Stopwatch.StartNew();
                var searchResults1 = sequences.GetAllPartiallyMatchingSequences0(partialSequence); sw1.Stop();

                var sw2 = Stopwatch.StartNew();
                var searchResults2 = sequences.GetAllPartiallyMatchingSequences1(partialSequence); sw2.Stop();

                Assert.IsTrue(searchResults1.Count == 1 && balancedVariant == searchResults1[0]);

                Assert.IsTrue(searchResults2.Count == 1 && balancedVariant == searchResults2[0]);

                for (int i = 0; i < sequenceLength; i++)
                    links.Delete(sequence[i]);
            }

            File.Delete(tempFilename);
        }
    }
}
