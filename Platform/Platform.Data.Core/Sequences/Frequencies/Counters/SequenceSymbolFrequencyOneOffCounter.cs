using System.Collections.Generic;
using Platform.Interfaces;
using Platform.Helpers.Numbers;
using Platform.Data.Core.Doublets;

namespace Platform.Data.Core.Sequences.Frequencies.Counters
{
    public class SequenceSymbolFrequencyOneOffCounter<TLink> : ICounter<TLink>
    {
        private static readonly EqualityComparer<TLink> EqualityComparer = EqualityComparer<TLink>.Default;
        private static readonly Comparer<TLink> Comparer = Comparer<TLink>.Default;

        protected readonly ILinks<TLink> _links;
        protected readonly TLink _sequenceLink;
        protected readonly TLink _symbol;
        protected TLink _total;

        public SequenceSymbolFrequencyOneOffCounter(ILinks<TLink> links, TLink sequenceLink, TLink symbol)
        {
            _links = links;
            _sequenceLink = sequenceLink;
            _symbol = symbol;
            _total = default;
        }

        public virtual TLink Count()
        {
            if (Comparer.Compare(_total, default) > 0)
                return _total;
            StopableSequenceWalker.WalkRight(_sequenceLink, _links.GetSource, _links.GetTarget, IsElement, VisitElement);
            return _total;
        }

        private bool IsElement(TLink x) => EqualityComparer.Equals(x, _symbol) || _links.IsPartialPoint(x); // TODO: Use SequenceElementCreteriaMatcher instead of IsPartialPoint

        private bool VisitElement(TLink element)
        {
            if (EqualityComparer.Equals(element, _symbol))
                _total = ArithmeticHelpers.Increment(_total);
            return true;
        }
    }
}
