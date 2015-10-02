using System.Diagnostics;
using System.IO;
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

                var elapsed1 = sw1.Elapsed;

                var sw2 = Stopwatch.StartNew();

                var results2 = sequences.CreateAllVariants2(sequence); sw2.Stop();

                var elapsed2 = sw2.Elapsed;

                Assert.IsTrue(results1.Count > results2.Length);
                Assert.IsTrue(elapsed1 > elapsed2);

                for (int i = 0; i < sequenceLength; i++)
                    links.Delete(sequence[i]);
            }

            File.Delete(tempFilename);
        }
    }
}
