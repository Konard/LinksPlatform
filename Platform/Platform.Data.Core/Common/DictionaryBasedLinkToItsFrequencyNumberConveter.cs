using System.Collections.Generic;
using Platform.Helpers;
using Platform.Helpers.Collections;

namespace Platform.Data.Core.Common
{
    public class DictionaryBasedLinkToItsFrequencyNumberConveter<TLink> : IConverter<TLink>
    {
        private readonly IDictionary<TLink, TLink> _linksToFrequencies;

        public DictionaryBasedLinkToItsFrequencyNumberConveter(IDictionary<TLink, TLink> linksToFrequencies) => _linksToFrequencies = linksToFrequencies;

        public TLink Convert(TLink link) => _linksToFrequencies.GetOrDefault(link);
    }
}
