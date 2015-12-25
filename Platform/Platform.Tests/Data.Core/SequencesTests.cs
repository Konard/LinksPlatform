using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Platform.Data.Core;
using Platform.Data.Core.Collections;
using Platform.Data.Core.Pairs;
using Platform.Data.Core.Sequences;
using Platform.Helpers;

namespace Platform.Tests.Data.Core
{
    [TestClass]
    public class SequencesTests
    {
        public static readonly long DefaultLinksSizeStep = LinksMemoryManager.LinkSizeInBytes * 1024 * 1024;

        [TestMethod]
        public void CreateAllVariantsTest()
        {
            var tempFilename = Path.GetTempFileName();

            const long sequenceLength = 8;

            const ulong itself = LinksConstants.Itself;

            using (var memoryManager = new LinksMemoryManager(tempFilename, DefaultLinksSizeStep))
            using (var links = new Links(memoryManager))
            {
                var sequence = new ulong[sequenceLength];
                for (var i = 0; i < sequenceLength; i++)
                    sequence[i] = links.Create(itself, itself);

                var sequences = new Sequences(links);

                var sw1 = Stopwatch.StartNew();
                var results1 = sequences.CreateAllVariants1(sequence); sw1.Stop();

                var sw2 = Stopwatch.StartNew();
                var results2 = sequences.CreateAllVariants2(sequence); sw2.Stop();

                Assert.IsTrue(results1.Count > results2.Length);
                Assert.IsTrue(sw1.Elapsed > sw2.Elapsed);

                for (var i = 0; i < sequenceLength; i++)
                    links.Delete(sequence[i]);
            }

            File.Delete(tempFilename);
        }

        [TestMethod]
        public void AllVariantsSearchTest()
        {
            var tempFilename = Path.GetTempFileName();

            const long sequenceLength = 8;

            const ulong itself = LinksConstants.Itself;

            using (var memoryManager = new LinksMemoryManager(tempFilename, DefaultLinksSizeStep))
            using (var links = new Links(memoryManager))
            {
                var sequence = new ulong[sequenceLength];
                for (var i = 0; i < sequenceLength; i++)
                    sequence[i] = links.Create(itself, itself);

                var sequences = new Sequences(links);

                var createResults = sequences.CreateAllVariants2(sequence).Distinct().ToArray();

                //for (int i = 0; i < createResults.Length; i++)
                //    sequences.Create(createResults[i]);

                var sw0 = Stopwatch.StartNew();
                var searchResults0 = sequences.GetAllMatchingSequences0(sequence); sw0.Stop();

                var sw1 = Stopwatch.StartNew();
                var searchResults1 = sequences.GetAllMatchingSequences1(sequence); sw1.Stop();

                var sw2 = Stopwatch.StartNew();
                var searchResults2 = sequences.Each1(sequence); sw2.Stop();

                var sw3 = Stopwatch.StartNew();
                var searchResults3 = sequences.Each(sequence); sw3.Stop();

                var intersection0 = createResults.Intersect(searchResults0).ToList();
                Assert.IsTrue(intersection0.Count == searchResults0.Count);
                Assert.IsTrue(intersection0.Count == createResults.Length);

                var intersection1 = createResults.Intersect(searchResults1).ToList();
                Assert.IsTrue(intersection1.Count == searchResults1.Count);
                Assert.IsTrue(intersection1.Count == createResults.Length);

                var intersection2 = createResults.Intersect(searchResults2).ToList();
                Assert.IsTrue(intersection2.Count == searchResults2.Count);
                Assert.IsTrue(intersection2.Count == createResults.Length);

                var intersection3 = createResults.Intersect(searchResults3).ToList();
                Assert.IsTrue(intersection3.Count == searchResults3.Count);
                Assert.IsTrue(intersection3.Count == createResults.Length);

                for (var i = 0; i < sequenceLength; i++)
                    links.Delete(sequence[i]);
            }

            File.Delete(tempFilename);
        }

        [TestMethod]
        public void BalancedVariantSearchTest()
        {
            var tempFilename = Path.GetTempFileName();

            const long sequenceLength = 200;

            const ulong itself = LinksConstants.Itself;

            using (var memoryManager = new LinksMemoryManager(tempFilename, DefaultLinksSizeStep))
            using (var links = new Links(memoryManager))
            {
                var sequence = new ulong[sequenceLength];
                for (var i = 0; i < sequenceLength; i++)
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

                Assert.IsTrue(searchResults3.Count == 1 && balancedVariant == searchResults3.First());

                //Assert.IsTrue(sw1.Elapsed < sw2.Elapsed);

                for (var i = 0; i < sequenceLength; i++)
                    links.Delete(sequence[i]);
            }

            File.Delete(tempFilename);
        }

        [TestMethod]
        public void AllPartialVariantsSearchTest()
        {
            var tempFilename = Path.GetTempFileName();

            const long sequenceLength = 8;

            const ulong itself = LinksConstants.Itself;

            using (var memoryManager = new LinksMemoryManager(tempFilename, DefaultLinksSizeStep))
            using (var links = new Links(memoryManager))
            {
                var sequence = new ulong[sequenceLength];
                for (var i = 0; i < sequenceLength; i++)
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
                Assert.IsTrue(intersection1.Count == createResults.Length);

                var intersection2 = createResults.Intersect(searchResults2).ToList();
                Assert.IsTrue(intersection2.Count == createResults.Length);

                for (var i = 0; i < sequenceLength; i++)
                    links.Delete(sequence[i]);
            }

            File.Delete(tempFilename);
        }

        [TestMethod]
        public void BalancedPartialVariantsSearchTest()
        {
            var tempFilename = Path.GetTempFileName();

            const long sequenceLength = 200;

            const ulong itself = LinksConstants.Itself;

            using (var memoryManager = new LinksMemoryManager(tempFilename, DefaultLinksSizeStep))
            using (var links = new Links(memoryManager))
            {
                var sequence = new ulong[sequenceLength];
                for (var i = 0; i < sequenceLength; i++)
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

                Assert.IsTrue(searchResults2.Count == 1 && balancedVariant == searchResults2.First());

                for (var i = 0; i < sequenceLength; i++)
                    links.Delete(sequence[i]);
            }

            File.Delete(tempFilename);
        }

        [TestMethod]
        public void PatternMatchTest()
        {
            var tempFilename = Path.GetTempFileName();

            const ulong itself = LinksConstants.Itself;
            const ulong one = LinksConstants.Any;
            const ulong zeroOrMany = Sequences.ZeroOrMany;

            using (var memoryManager = new LinksMemoryManager(tempFilename, DefaultLinksSizeStep))
            using (var links = new Links(memoryManager))
            {
                var e1 = links.Create(itself, itself);
                var e2 = links.Create(itself, itself);

                var sequence = new[]
                {
                    e1, e2, e1, e2 // mama / papa
                };

                var sequences = new Sequences(links);

                var balancedVariant = sequences.CreateBalancedVariant(sequence);

                // 1: [1]
                // 2: [2]
                // 3: [1,2]
                // 4: [1,2,1,2]

                var pair = links.GetSource(balancedVariant);

                var matchedSequences1 = sequences.MatchPattern(e2, e1, zeroOrMany);

                Assert.IsTrue(matchedSequences1.Count == 0);

                var matchedSequences2 = sequences.MatchPattern(zeroOrMany, e2, e1);

                Assert.IsTrue(matchedSequences2.Count == 0);

                var matchedSequences3 = sequences.MatchPattern(e1, zeroOrMany, e1);

                Assert.IsTrue(matchedSequences3.Count == 0);

                var matchedSequences4 = sequences.MatchPattern(e1, zeroOrMany, e2);

                Assert.IsTrue(matchedSequences4.Contains(pair));
                Assert.IsTrue(matchedSequences4.Contains(balancedVariant));

                for (var i = 0; i < sequence.Length; i++)
                    links.Delete(sequence[i]);
            }

            File.Delete(tempFilename);
        }

        private static void InitBitString()
        {
            Global.Trash = new BitString(1);
        }

        [TestMethod]
        public void CompressionTest()
        {
            var tempFilename = Path.GetTempFileName();

            const ulong itself = LinksConstants.Itself;

            using (var memoryManager = new LinksMemoryManager(tempFilename, DefaultLinksSizeStep))
            using (var links = new Links(memoryManager))
            {
                var e1 = links.Create(itself, itself);
                var e2 = links.Create(itself, itself);

                var sequence = new[]
                {
                    e1, e2, e1, e2 // mama / papa / template [(m/p), a] { [1] [2] [1] [2] }
                };

                var sequences = new Sequences(links);

                var compressor = new Compressor(links, sequences);

                var compressedVariant = compressor.Compress(sequence);

                // 1: [1]       (1->1) point
                // 2: [2]       (2->2) point
                // 3: [1,2]     (1->2) pair
                // 4: [1,2,1,2] (3->3) pair

                Assert.IsTrue(links.GetSource(links.GetSource(compressedVariant)) == sequence[0]);
                Assert.IsTrue(links.GetTarget(links.GetSource(compressedVariant)) == sequence[1]);
                Assert.IsTrue(links.GetSource(links.GetTarget(compressedVariant)) == sequence[2]);
                Assert.IsTrue(links.GetTarget(links.GetTarget(compressedVariant)) == sequence[3]);
            }

            File.Delete(tempFilename);
        }

        [TestMethod]
        public void AllPossibleConnectionsTest()
        {
            InitBitString();

            var tempFilename = Path.GetTempFileName();

            const long sequenceLength = 5;

            const ulong itself = LinksConstants.Itself;

            using (var memoryManager = new LinksMemoryManager(tempFilename, DefaultLinksSizeStep))
            using (var links = new Links(memoryManager))
            {
                var sequence = new ulong[sequenceLength];
                for (var i = 0; i < sequenceLength; i++)
                    sequence[i] = links.Create(itself, itself);

                var sequences = new Sequences(links);

                var createResults = sequences.CreateAllVariants2(sequence);

                var reverseResults = sequences.CreateAllVariants2(sequence.Reverse().ToArray());

                for (var i = 0; i < 1; i++)
                {
                    var sw1 = Stopwatch.StartNew();
                    var searchResults1 = sequences.GetAllConnections(sequence); sw1.Stop();

                    var sw2 = Stopwatch.StartNew();
                    var searchResults2 = sequences.GetAllConnections1(sequence); sw2.Stop();

                    var sw3 = Stopwatch.StartNew();
                    var searchResults3 = sequences.GetAllConnections2(sequence); sw3.Stop();

                    var sw4 = Stopwatch.StartNew();
                    var searchResults4 = sequences.GetAllConnections3(sequence); sw4.Stop();

                    Global.Trash = searchResults3;
                    Global.Trash = searchResults4;

                    var intersection1 = createResults.Intersect(searchResults1).ToList();
                    Assert.IsTrue(intersection1.Count == createResults.Length);

                    var intersection2 = reverseResults.Intersect(searchResults1).ToList();
                    Assert.IsTrue(intersection2.Count == reverseResults.Length);

                    var intersection0 = searchResults1.Intersect(searchResults2).ToList();
                    Assert.IsTrue(intersection0.Count == searchResults2.Count);

                    var intersection3 = searchResults2.Intersect(searchResults3).ToList();
                    Assert.IsTrue(intersection3.Count == searchResults3.Count);

                    var intersection4 = searchResults3.Intersect(searchResults4).ToList();
                    Assert.IsTrue(intersection4.Count == searchResults4.Count);
                }

                for (var i = 0; i < sequenceLength; i++)
                    links.Delete(sequence[i]);
            }

            File.Delete(tempFilename);
        }

        [TestMethod]
        public void CalculateAllUsagesTest()
        {
            InitBitString();

            var tempFilename = Path.GetTempFileName();

            const long sequenceLength = 3;

            const ulong itself = LinksConstants.Itself;

            using (var memoryManager = new LinksMemoryManager(tempFilename, DefaultLinksSizeStep))
            using (var links = new Links(memoryManager))
            {
                var sequence = new ulong[sequenceLength];
                for (var i = 0; i < sequenceLength; i++)
                    sequence[i] = links.Create(itself, itself);

                var sequences = new Sequences(links);

                var createResults = sequences.CreateAllVariants2(sequence);

                //var reverseResults = sequences.CreateAllVariants2(sequence.Reverse().ToArray());

                for (var i = 0; i < 1; i++)
                {
                    var linksTotalUsages1 = new ulong[links.Count() + 1];

                    sequences.CalculateAllUsages(linksTotalUsages1);

                    var linksTotalUsages2 = new ulong[links.Count() + 1];

                    sequences.CalculateAllUsages2(linksTotalUsages2);

                    var intersection1 = linksTotalUsages1.Intersect(linksTotalUsages2).ToList();
                    Assert.IsTrue(intersection1.Count == linksTotalUsages2.Length);
                }

                for (var i = 0; i < sequenceLength; i++)
                    links.Delete(sequence[i]);
            }

            File.Delete(tempFilename);
        }
    }
}
