using Platform.Data.Core.Pairs;
using Platform.Data.Core.Sequences;

namespace Platform.Sandbox
{
    public class WikipediaLinksStorage : IWikipediaStorage<ulong>
    {
        private readonly Sequences _sequences;
        private readonly Links _links;
        private ulong _elementMarker;
        private ulong _textElementMarker;

        public WikipediaLinksStorage(Sequences sequences)
        {
            _sequences = sequences;
            _links = sequences.Links;

            InitConstants();
        }

        private void InitConstants()
        {
            var markerIndex = UnicodeMap.LastCharLink + 1;

            _elementMarker = CreateConstant(markerIndex++);
            _textElementMarker = CreateConstant(markerIndex++);

            // Reserve 100 more
            for (var i = 0; i < 100; i++)
                CreateConstant(markerIndex++);
        }

        private ulong CreateConstant(ulong markerIndex)
        {
            if (!_links.Exists(markerIndex))
                _links.Create(LinksConstants.Itself, LinksConstants.Itself);
            return markerIndex;
        }

        public ulong CreateElement(string name)
        {
            var nameLinks = UnicodeMap.FromStringToLinkArray(name);
            var nameSequence = _sequences.Create(nameLinks);

            return _links.Create(_elementMarker, nameSequence);
        }

        public ulong CreateTextElement(string content)
        {
            var contentLinks = UnicodeMap.FromStringToLinkArray(content);
            var contentSequence = _sequences.Create(contentLinks);

            return _links.Create(_textElementMarker, contentSequence);
        }

        public void AttachElementToParent(ulong elementToAttach, ulong parent)
        {
            _links.Create(parent, elementToAttach);
        }
    }
}

