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
    public class OptimalVariantSequenceTests
    {
        [Fact]
        public void OptimalVariantSequenceTest()
        {
            const long sequenceLength = 200;

            using (var scope = new TempLinksTestScope(useSequences: true))
            {
                var links = scope.Links;
                var sequences = scope.Sequences;
                var constants = Default<LinksConstants<bool, ulong, int>>.Instance;

                var sequence = new ulong[sequenceLength];
                for (var i = 0; i < sequenceLength; i++)
                    sequence[i] = links.Create();
                    
                var meaningRoot = links.Create();
                var unaryOne = links.Create(meaningRoot, constants.Itself);
                var frequencyMarker = links.Create(meaningRoot, constants.Itself);
                
                sequences.SetUnaryOne(unaryOne);
                sequences.SetFrequencyMarker(frequencyMarker);


                var sw1 = Stopwatch.StartNew();
                sequences.IncrementPairsFrequencies(sequence); sw1.Stop();

                

                for (var i = 0; i < sequenceLength; i++)
                    links.Delete(sequence[i]);
            }
        }
    }
}
