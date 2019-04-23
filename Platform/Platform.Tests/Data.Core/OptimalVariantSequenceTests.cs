using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
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
            sequenceToItsLocalElementLevelsConverter.IncrementDoubletsFrequencies(sequence);

            var levels = sequenceToItsLocalElementLevelsConverter.Convert(sequence);

            var optimalVariant = optimalVariantConverter.Convert(sequence);

            var readSequence1 = sequences.ReadSequenceCore(optimalVariant, links.IsPartialPoint);

            Assert.True(sequence.SequenceEqual(readSequence1));
        }
    }
}
