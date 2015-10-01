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

            const long sequenceLength = 9;

            const ulong itself = Pairs.Links.Itself;

            using (var links = new Pairs.Links(tempFilename, 1024 * 1024))
            {
                var sequence = new ulong[sequenceLength];
                for (int i = 0; i < sequenceLength; i++)
                    sequence[i] = links.Create(itself, itself);

                var sequences = new Sequences(links);

                var results = sequences.CreateAllVariants2(sequence);

                for (int i = 0; i < sequenceLength; i++)
                    links.Delete(sequence[i]);
            }

            File.Delete(tempFilename);
        }
    }
}
