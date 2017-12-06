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
                
                links.UseUnicode();
                
                var sequence = UnicodeMap.FromStringToLinkArray("зеленела зелёная зелень");
                
                var meaningRoot = links.CreatePoint();
                var unaryOne = links.CreateAndUpdate(meaningRoot, constants.Itself);
                var frequencyMarker = links.CreateAndUpdate(meaningRoot, constants.Itself);
                
                sequences.SetUnaryOne(unaryOne);
                sequences.SetFrequencyMarker(frequencyMarker);

                var sw1 = Stopwatch.StartNew();
                sequences.IncrementPairsFrequencies(sequence); sw1.Stop();   
                
                sequences.PrintPairsFrequencies(sequence);
                
                var levels = sequences.CalculateLocalElementLevels(sequence);
                
                for (var i = 0; i < sequence.Length; i++)
                    Console.WriteLine("sequence[{0}] = {1}({2})", i, sequence[i], UnicodeMap.FromLinkToChar(sequence[i]));
                
                for (var i = 0; i < levels.Length; i++)
                    Console.WriteLine("levels[{0}] = {1}", i, levels[i]);
                
                var optimalVariant = sequences.CreateOptimalVariant(sequence);
                
                Console.WriteLine("optimalVariant = {0}", optimalVariant);
                
                var sw3 = Stopwatch.StartNew();
                var readSequence1 = sequences.ReadSequenceCore(optimalVariant, links.IsPartialPoint); sw3.Stop();
               
                Assert.True(sequence.SequenceEqual(readSequence1));
            }
        }
    }
}
