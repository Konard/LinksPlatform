using System.Collections.Generic;
using Platform.Numbers;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Numbers.Raw;
using Platform.Data.Doublets.Sequences.Converters;
using Platform.Data.Doublets.Sequences.Frequencies.Cache;
using Platform.Data.Doublets.Sequences.Indexes;
using Platform.Data.Doublets.Unicode;

namespace Platform.Examples
{
    public class WikipediaLinksStorage : IWikipediaStorage<ulong>
    {
        private readonly StringToUnicodeSequenceConverter<ulong> _stringToUnicodeSequenceConverter;
        private readonly ILinks<ulong> _links;
        private ulong _unicodeSymbolMarker;
        private ulong _unicodeSequenceMarker;
        private ulong _elementMarker;
        private ulong _textElementMarker;
        private ulong _documentMarker;

        private class Unindex : ISequenceIndex<ulong>
        {
            public bool Add(IList<ulong> sequence) => true;
            public bool MightContain(IList<ulong> sequence) => false;
        }

        public WikipediaLinksStorage(ILinks<ulong> links, LinkFrequenciesCache<ulong> frequenciesCache)
        {
            var linkToItsFrequencyNumberConverter = new FrequenciesCacheBasedLinkToItsFrequencyNumberConverter<ulong>(frequenciesCache);
            var sequenceToItsLocalElementLevelsConverter = new SequenceToItsLocalElementLevelsConverter<ulong>(links, linkToItsFrequencyNumberConverter);
            var optimalVariantConverter = new OptimalVariantConverter<ulong>(links, sequenceToItsLocalElementLevelsConverter);
            InitConstants(links);
            var charToUnicodeSymbolConverter = new CharToUnicodeSymbolConverter<ulong>(links, new AddressToRawNumberConverter<ulong>(), _unicodeSymbolMarker);
            _stringToUnicodeSequenceConverter = new StringToUnicodeSequenceConverter<ulong>(links, charToUnicodeSymbolConverter, new Unindex(), optimalVariantConverter, _unicodeSequenceMarker);
            _links = links;
        }

        private void InitConstants(ILinks<ulong> links)
        {
            var markerIndex = 1UL;
            var meaningRoot = links.GetOrCreate(markerIndex, markerIndex);
            _unicodeSymbolMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
            _unicodeSequenceMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
            _elementMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
            _textElementMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
            _documentMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
        }

        public ulong CreateDocument(string name) => Create(_documentMarker, name);

        public ulong CreateElement(string name) => Create(_elementMarker, name);

        public ulong CreateTextElement(string content) => Create(_textElementMarker, content);

        private ulong Create(ulong marker, string content)
        {
            var contentSequence = _stringToUnicodeSequenceConverter.Convert(content);
            return _links.GetOrCreate(marker, contentSequence);
        }

        public void AttachElementToParent(ulong elementToAttach, ulong parent) => _links.GetOrCreate(parent, elementToAttach);
    }
}

