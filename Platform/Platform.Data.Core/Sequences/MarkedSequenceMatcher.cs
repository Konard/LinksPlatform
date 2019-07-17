using System.Collections.Generic;
using Platform.Interfaces;
using Platform.Data.Core.Doublets;

namespace Platform.Data.Core.Sequences
{
    public class MarkedSequenceMatcher<TLink> : ICreteriaMatcher<TLink>
    {
        private static readonly EqualityComparer<TLink> EqualityComparer = EqualityComparer<TLink>.Default;

        private readonly ILinks<TLink> _links;
        private readonly TLink _sequenceMarkerLink;

        public MarkedSequenceMatcher(ILinks<TLink> links, TLink sequenceMarkerLink)
        {
            _links = links;
            _sequenceMarkerLink = sequenceMarkerLink;
        }

        public bool IsMatched(TLink sequenceCandidate) =>
            EqualityComparer.Equals(_links.GetSource(sequenceCandidate), _sequenceMarkerLink)
        || !EqualityComparer.Equals(_links.SearchOrDefault(_sequenceMarkerLink, sequenceCandidate), _links.Constants.Null);
    }
}
