using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Platform.Data.Core.Collections;
using Platform.Data.Core.Pairs;
using Platform.Data.Core.Sequences;
using Platform.Helpers;
using Xunit;

namespace Platform.Tests.Data.Core
{
    public class ReadSequnceTests
    {
        [Fact]
        public void ReadSequenceTest()
        {
            const long sequenceLength = 200;

            using (var scope = new TempLinksTestScope(useSequences: true))
            {
                var links = scope.Links;
                var sequences = scope.Sequences;

                var sequence = new ulong[sequenceLength];
                for (var i = 0; i < sequenceLength; i++)
                    sequence[i] = links.Create();

                var sw1 = Stopwatch.StartNew();
                var balancedVariant = sequences.CreateBalancedVariant(sequence); sw1.Stop();

                var sw2 = Stopwatch.StartNew();
                var readSequence = sequences.ReadSequenceCore(balancedVariant, links.IsPartialPoint); sw2.Stop();

                // var sw3 = Stopwatch.StartNew();
                // var searchResults3 = sequences.GetAllMatchingSequences1(sequence); sw3.Stop();

                // На количестве в 200 элементов это будет занимать вечность
                //var sw4 = Stopwatch.StartNew();
                //var searchResults4 = sequences.Each(sequence); sw4.Stop();

                Assert.True(sequence.Length == readSequence.Length); //Count == 1 && balancedVariant == searchResults2[0]);

                //Assert.True(searchResults3.Count == 1 && balancedVariant == searchResults3.First());

                //Assert.True(sw1.Elapsed < sw2.Elapsed);

                for (var i = 0; i < sequenceLength; i++)
                    links.Delete(sequence[i]);
            }
        }
    }
}
