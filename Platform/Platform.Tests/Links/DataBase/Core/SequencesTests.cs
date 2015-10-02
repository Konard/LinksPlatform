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

                var createResults = sequences.CreateAllVariants2(sequence);

                var sw1 = Stopwatch.StartNew();
                var searchResults1 = sequences.GetAllMatchingSequences(sequence); sw1.Stop();

                var sw2 = Stopwatch.StartNew();
                var searchResults2 = sequences.Each(sequence); sw2.Stop();

                var intersection1 = createResults.Intersect(searchResults1).ToList();
                Assert.IsTrue(intersection1.Count == searchResults1.Count);

                var intersection2 = createResults.Intersect(searchResults2).ToList();
                Assert.IsTrue(intersection2.Count == searchResults2.Count);

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

            const long sequenceLength = 50;

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
                var searchResults = sequences.GetAllMatchingSequences(sequence); sw2.Stop();

                Assert.IsTrue(searchResults.Count == 1 && balancedVariant == searchResults[0]);

                //Assert.IsTrue(sw1.Elapsed < sw2.Elapsed);

                for (int i = 0; i < sequenceLength; i++)
                    links.Delete(sequence[i]);
            }

            File.Delete(tempFilename);
        }
    }
}
