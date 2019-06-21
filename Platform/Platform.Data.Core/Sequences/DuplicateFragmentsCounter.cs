using System.Collections.Generic;
using System.Linq;
using Platform.Helpers;

namespace Platform.Data.Core.Sequences
{
    public class DuplicateFragmentsCounter<TLink> : ICounter<int>
    {
        private readonly IProvider<IList<KeyValuePair<IList<TLink>, IList<TLink>>>> _duplicateFragmentsProvider;

        public DuplicateFragmentsCounter(IProvider<IList<KeyValuePair<IList<TLink>, IList<TLink>>>> duplicateFragmentsProvider) => _duplicateFragmentsProvider = duplicateFragmentsProvider;

        public int Count() => _duplicateFragmentsProvider.Get().Sum(x => x.Value.Count);
    }
}
