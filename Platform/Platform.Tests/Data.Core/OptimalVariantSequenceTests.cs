using System;
using System.Diagnostics;
using System.Linq;
using Xunit;
using Platform.Helpers;
using Platform.Data.Core.Doublets;
using Platform.Data.Core.Sequences;

namespace Platform.Tests.Data.Core
{
    public class OptimalVariantSequenceTests
    {
        [Fact]
        public void OptimalVariantSequenceTest()
        {
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
                var frequencyPropertyMarker = links.CreateAndUpdate(meaningRoot, constants.Itself);

                var unaryNumberToAddressConveter = new UnaryNumberToAddressAddOperationConverter<ulong>(links, unaryOne);
                var unaryNumberIncrementer = new UnaryNumberIncrementer<ulong>(links, unaryOne);
                var frequencyIncrementer = new FrequencyIncrementer<ulong>(links, frequencyMarker, unaryOne, unaryNumberIncrementer);
                var frequencyPropertyOperator = new FrequencyPropertyOperator<ulong>(links, frequencyPropertyMarker, frequencyMarker);
                var linkFrequencyIncrementer = new LinkFrequencyIncrementer<ulong>(links, frequencyPropertyOperator, frequencyIncrementer);
                var linkToItsFrequencyNumberConverter = new LinkToItsFrequencyNumberConveter<ulong>(links, frequencyPropertyOperator, unaryNumberToAddressConveter);
                var sequenceToItsLocalElementLevelsConverter = new SequenceToItsLocalElementLevelsConverter<ulong>(links, linkFrequencyIncrementer, linkToItsFrequencyNumberConverter);
                var optimalVariantConverter = new OptimalVariantConverter<ulong>(links, sequenceToItsLocalElementLevelsConverter);
                
                var sw1 = Stopwatch.StartNew();
                sequenceToItsLocalElementLevelsConverter.IncrementDoubletsFrequencies(sequence); sw1.Stop();

                sequenceToItsLocalElementLevelsConverter.PrintDoubletsFrequencies(sequence);
                
                var levels = sequenceToItsLocalElementLevelsConverter.Convert(sequence);
                
                for (var i = 0; i < sequence.Length; i++)
                    Console.WriteLine("sequence[{0}] = {1}({2})", i, sequence[i], UnicodeMap.FromLinkToChar(sequence[i]));
                
                for (var i = 0; i < levels.Count; i++)
                    Console.WriteLine("levels[{0}] = {1}", i, levels[i]);
                
                var optimalVariant = optimalVariantConverter.Convert(sequence);
                
                Console.WriteLine("optimalVariant = {0}", optimalVariant);
                
                var sw3 = Stopwatch.StartNew();
                var readSequence1 = sequences.ReadSequenceCore(optimalVariant, links.IsPartialPoint); sw3.Stop();
               
                Assert.True(sequence.SequenceEqual(readSequence1));
            }
        }
    }
}
