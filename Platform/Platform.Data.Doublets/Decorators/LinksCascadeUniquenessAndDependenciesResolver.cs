using System.Collections.Generic;
using Platform.Collections.Arrays;
using Platform.Numbers;

namespace Platform.Data.Doublets.Decorators
{
    public class LinksCascadeUniquenessAndDependenciesResolver<TLink> : LinksUniquenessResolver<TLink>
    {
        private static readonly EqualityComparer<TLink> _equalityComparer = EqualityComparer<TLink>.Default;

        public LinksCascadeUniquenessAndDependenciesResolver(ILinks<TLink> links) : base(links) { }

        protected override TLink ResolveAddressChangeConflict(TLink oldLinkAddress, TLink newLinkAddress)
        {
            // TODO: Very similar to Merge (logic should be reused)
            ulong referencesAsSourceCount = (Integer<TLink>)Links.Count(Constants.Any, oldLinkAddress, Constants.Any);
            ulong referencesAsTargetCount = (Integer<TLink>)Links.Count(Constants.Any, Constants.Any, oldLinkAddress);
            var references = ArrayPool.Allocate<TLink>((long)(referencesAsSourceCount + referencesAsTargetCount));
            var referencesFiller = new ArrayFiller<TLink, TLink>(references, Constants.Continue);
            Links.Each(referencesFiller.AddFirstAndReturnConstant, Constants.Any, oldLinkAddress, Constants.Any);
            Links.Each(referencesFiller.AddFirstAndReturnConstant, Constants.Any, Constants.Any, oldLinkAddress);
            for (ulong i = 0; i < referencesAsSourceCount; i++)
            {
                var reference = references[i];
                if (!_equalityComparer.Equals(reference, oldLinkAddress))
                {
                    Links.Update(reference, newLinkAddress, Links.GetTarget(reference));
                }
            }
            for (var i = (long)referencesAsSourceCount; i < references.Length; i++)
            {
                var reference = references[i];
                if (!_equalityComparer.Equals(reference, oldLinkAddress))
                {
                    Links.Update(reference, Links.GetSource(reference), newLinkAddress);
                }
            }
            ArrayPool.Free(references);
            return base.ResolveAddressChangeConflict(oldLinkAddress, newLinkAddress);
        }
    }
}
