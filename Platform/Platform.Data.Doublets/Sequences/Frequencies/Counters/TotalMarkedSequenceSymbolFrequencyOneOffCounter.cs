using Platform.Interfaces;
using Platform.Numbers;

namespace Platform.Data.Doublets.Sequences.Frequencies.Counters
{
    public class TotalMarkedSequenceSymbolFrequencyOneOffCounter<TLink> : TotalSequenceSymbolFrequencyOneOffCounter<TLink>
    {
        private readonly ICreteriaMatcher<TLink> _markedSequenceMatcher;

        public TotalMarkedSequenceSymbolFrequencyOneOffCounter(ILinks<TLink> links, ICreteriaMatcher<TLink> markedSequenceMatcher, TLink symbol) : base(links, symbol)
            => _markedSequenceMatcher = markedSequenceMatcher;

        protected override void CountSequenceSymbolFrequency(TLink link)
        {
            var symbolFrequencyCounter = new MarkedSequenceSymbolFrequencyOneOffCounter<TLink>(_links, _markedSequenceMatcher, link, _symbol);
            _total = ArithmeticHelpers.Add(_total, symbolFrequencyCounter.Count());
        }
    }
}
