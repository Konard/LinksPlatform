using System.Collections.Generic;
using Platform.Helpers;

namespace Platform.Data.Core.Common
{
    public class DictionaryBasedLinkFrequencyIncrementer<TLink> : IIncrementer<TLink>
    {
        private readonly IDictionary<TLink, TLink> _linksToFrequencies;

        public DictionaryBasedLinkFrequencyIncrementer(IDictionary<TLink, TLink> linksToFrequencies) => _linksToFrequencies = linksToFrequencies;

        /// <remarks>Link itseft is not changed, only it's frequency is incremented.</remarks>
        public TLink Increment(TLink link)
        {
            if (_linksToFrequencies.TryGetValue(link, out TLink frequency))
            {
                frequency = MathHelpers.Increment(frequency);
                _linksToFrequencies[link] = frequency;
            }
            else
            {
                frequency = Integer<TLink>.One;
                _linksToFrequencies.Add(link, frequency);
            }
            return link;
        }
    }
}
