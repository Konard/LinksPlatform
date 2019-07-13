using Platform.Interfaces;
using Platform.Helpers.Numbers;
using Platform.Data.Core.Doublets;

namespace Platform.Data.Core.Sequences
{
    public class MarkedSequenceMatcher<TLink> : ICreteriaMatcher<TLink>
    {
        private readonly ILinks<TLink> _links;
        private readonly TLink _sequenceMarkerLink;

        public MarkedSequenceMatcher(ILinks<TLink> links, TLink sequenceMarkerLink)
        {
            _links = links;
            _sequenceMarkerLink = sequenceMarkerLink;
        }

        public bool IsMatched(TLink sequenceCandidate) =>
            MathHelpers<TLink>.IsEquals(_links.GetSource(sequenceCandidate), _sequenceMarkerLink)
        || !MathHelpers<TLink>.IsEquals(_links.SearchOrDefault(_sequenceMarkerLink, sequenceCandidate), _links.Constants.Null);
    }
}
