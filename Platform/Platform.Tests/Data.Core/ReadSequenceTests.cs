using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;
using Platform.Data.Core.Doublets;
using Platform.Data.Core.Sequences;

namespace Platform.Tests.Data.Core
{
    public class ReadSequenceTests
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

                var balancedVariantConverter = new BalancedVariantConverter<ulong>(links);

                var sw1 = Stopwatch.StartNew();
                var balancedVariant = balancedVariantConverter.Convert(sequence); sw1.Stop();

                var sw2 = Stopwatch.StartNew();
                var readSequence1 = sequences.ReadSequenceCore(balancedVariant, links.IsPartialPoint); sw2.Stop();

                var sw3 = Stopwatch.StartNew();
                var readSequence2 = new List<ulong>();
                SequenceWalker.WalkRight(balancedVariant, 
                                         links.GetSource, 
                                         links.GetTarget,
                                         links.IsPartialPoint,
                                         readSequence2.Add);
                sw3.Stop();

                Assert.True(sequence.SequenceEqual(readSequence1));
                
                Assert.True(sequence.SequenceEqual(readSequence2)); 
             
                // Assert.True(sw2.Elapsed < sw3.Elapsed);
                
                Console.WriteLine($"Stack-based walker: {sw3.Elapsed}, Level-based reader: {sw2.Elapsed}");

                for (var i = 0; i < sequenceLength; i++)
                    links.Delete(sequence[i]);
            }
        }
    }
}
