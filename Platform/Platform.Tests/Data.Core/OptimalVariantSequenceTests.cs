using System;
using System.Diagnostics;
using System.Linq;
using Xunit;
using System.Collections.Generic;
using Platform.Data.Core.Common;
using Platform.Data.Core.Doublets;
using Platform.Data.Core.Sequences;

namespace Platform.Tests.Data.Core
{
    public class OptimalVariantSequenceTests
    {
        private const string SequenceExample = "зеленела зелёная зелень";

        [Fact]
        public void LinksBasedFrequencyStoredOptimalVariantSequenceTest()
        {
            using (var scope = new TempLinksTestScope(useSequences: true))
            {
                var links = scope.Links;
                var sequences = scope.Sequences;
                var constants = links.Constants;

                links.UseUnicode();

                var sequence = UnicodeMap.FromStringToLinkArray(SequenceExample);

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

                ExecuteTest(links, sequences, sequence, sequenceToItsLocalElementLevelsConverter, optimalVariantConverter);
            }
        }

        [Fact]
        public void DictionaryBasedFrequencyStoredOptimalVariantSequenceTest()
        {
            using (var scope = new TempLinksTestScope(useSequences: true))
            {
                var links = scope.Links;
                var sequences = scope.Sequences;

                links.UseUnicode();

                var sequence = UnicodeMap.FromStringToLinkArray(SequenceExample);

                var linksToFrequencies = new Dictionary<ulong, ulong>();

                var linkFrequencyIncrementer = new DictionaryBasedLinkFrequencyIncrementer<ulong>(linksToFrequencies);
                var linkToItsFrequencyNumberConverter = new DictionaryBasedLinkToItsFrequencyNumberConveter<ulong>(linksToFrequencies);
                var sequenceToItsLocalElementLevelsConverter = new SequenceToItsLocalElementLevelsConverter<ulong>(links, linkFrequencyIncrementer, linkToItsFrequencyNumberConverter);
                var optimalVariantConverter = new OptimalVariantConverter<ulong>(links, sequenceToItsLocalElementLevelsConverter);

                ExecuteTest(links, sequences, sequence, sequenceToItsLocalElementLevelsConverter, optimalVariantConverter);
            }
        }

        private static void ExecuteTest(SynchronizedLinks<ulong> links, Sequences sequences, ulong[] sequence, SequenceToItsLocalElementLevelsConverter<ulong> sequenceToItsLocalElementLevelsConverter, OptimalVariantConverter<ulong> optimalVariantConverter)
        {
            var sw1 = Stopwatch.StartNew();
            sequenceToItsLocalElementLevelsConverter.IncrementDoubletsFrequencies(sequence); sw1.Stop();

            sequenceToItsLocalElementLevelsConverter.PrintDoubletsFrequencies(sequence);

            var levels = sequenceToItsLocalElementLevelsConverter.Convert(sequence);

            for (var i = 0; i < sequence.Length; i++)
                Console.WriteLine("sequence[{0}] = {1}({2})", i, sequence[i], UnicodeMap.FromLinkToChar(sequence[i]));

            for (var i = 0; i < levels.Count; i++)
                Console.WriteLine("levels[{0}] = {1}", i, levels[i]);

            var sw2 = Stopwatch.StartNew();
            var optimalVariant = optimalVariantConverter.Convert(sequence); sw2.Stop();

            Console.WriteLine("optimalVariant = {0}", optimalVariant);

            var sw3 = Stopwatch.StartNew();
            var readSequence1 = sequences.ReadSequenceCore(optimalVariant, links.IsPartialPoint); sw3.Stop();

            Assert.True(sequence.SequenceEqual(readSequence1));
        }
    }
}
