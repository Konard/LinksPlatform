using Platform.Helpers;
using Platform.Data.Core.Doublets;

namespace Platform.Data.Core.Sequences
{
    public class DefaultSequenceRightHeightProvider<TLink> : LinksOperatorBase<TLink>, ISequenceHeightProvider<TLink>
    {
        private readonly ICreteriaMatcher<TLink> _elementMatcher;

        public DefaultSequenceRightHeightProvider(ILinks<TLink> links, ICreteriaMatcher<TLink> elementMatcher) : base(links)
        {
            _elementMatcher = elementMatcher;
        }

        public TLink Get(TLink sequence)
        {
            var height = default(TLink);
            var pairOrElement = sequence;
            while(!_elementMatcher.IsMatched(pairOrElement))
            {
                pairOrElement = Links.GetTarget(pairOrElement);
                height = MathHelpers.Increment(height);
            }
            return height;
        }
    }
}
