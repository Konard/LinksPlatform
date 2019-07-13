﻿using Platform.Interfaces;
using Platform.Helpers;
using Platform.Data.Core.Doublets;

namespace Platform.Data.Core.Sequences.Frequencies.Counters
{
    public class SequenceSymbolFrequencyOneOffCounter<TLink> : ICounter<TLink>
    {
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
            if (MathHelpers.GreaterThan(_total, default))
                return _total;
            StopableSequenceWalker.WalkRight(_sequenceLink, _links.GetSource, _links.GetTarget, IsElement, VisitElement);
            return _total;
        }

        private bool IsElement(TLink x) => MathHelpers<TLink>.IsEquals(x, _symbol) || _links.IsPartialPoint(x); // TODO: Use SequenceElementCreteriaMatcher instead of IsPartialPoint

        private bool VisitElement(TLink element)
        {
            if (MathHelpers<TLink>.IsEquals(element, _symbol))
                _total = MathHelpers.Increment(_total);
            return true;
        }
    }
}
