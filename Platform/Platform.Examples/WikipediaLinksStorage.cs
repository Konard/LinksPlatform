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
    public class WikipediaLinksStorage<TLink> : IWikipediaStorage<TLink>
    {
        private readonly StringToUnicodeSequenceConverter<TLink> _stringToUnicodeSequenceConverter;
        private readonly ILinks<TLink> _links;
        private TLink _unicodeSymbolMarker;
        private TLink _unicodeSequenceMarker;
        private TLink _elementMarker;
        private TLink _textElementMarker;
        private TLink _documentMarker;

        private class Unindex : ISequenceIndex<TLink>
        {
            public bool Add(IList<TLink> sequence) => true;
            public bool MightContain(IList<TLink> sequence) => false;
        }

        public WikipediaLinksStorage(ILinks<TLink> links, LinkFrequenciesCache<TLink> frequenciesCache)
        {
            var linkToItsFrequencyNumberConverter = new FrequenciesCacheBasedLinkToItsFrequencyNumberConverter<TLink>(frequenciesCache);
            var sequenceToItsLocalElementLevelsConverter = new SequenceToItsLocalElementLevelsConverter<TLink>(links, linkToItsFrequencyNumberConverter);
            var optimalVariantConverter = new OptimalVariantConverter<TLink>(links, sequenceToItsLocalElementLevelsConverter);
            InitConstants(links);
            var charToUnicodeSymbolConverter = new CharToUnicodeSymbolConverter<TLink>(links, new AddressToRawNumberConverter<TLink>(), _unicodeSymbolMarker);
            _stringToUnicodeSequenceConverter = new StringToUnicodeSequenceConverter<TLink>(links, charToUnicodeSymbolConverter, new Unindex(), optimalVariantConverter, _unicodeSequenceMarker);
            _links = links;
        }

        private void InitConstants(ILinks<TLink> links)
        {
            var markerIndex = Integer<TLink>.One;
            var meaningRoot = links.GetOrCreate(markerIndex, markerIndex);
            _unicodeSymbolMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
            _unicodeSequenceMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
            _elementMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
            _textElementMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
            _documentMarker = links.GetOrCreate(meaningRoot, Arithmetic.Increment(markerIndex));
        }

        public TLink CreateDocument(string name) => Create(_documentMarker, name);

        public TLink CreateElement(string name) => Create(_elementMarker, name);

        public TLink CreateTextElement(string content) => Create(_textElementMarker, content);

        private TLink Create(TLink marker, string content)
        {
            var contentSequence = _stringToUnicodeSequenceConverter.Convert(content);
            return _links.GetOrCreate(marker, contentSequence);
        }

        public void AttachElementToParent(TLink elementToAttach, TLink parent) => _links.GetOrCreate(parent, elementToAttach);
    }
}

