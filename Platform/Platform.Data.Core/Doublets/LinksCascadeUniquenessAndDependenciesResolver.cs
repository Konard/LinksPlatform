using Platform.Helpers;
using Platform.Helpers.Collections;

namespace Platform.Data.Core.Pairs
{
    public class LinksCascadeUniquenessAndDependenciesResolver<T> : LinksUniquenessResolver<T>
    {
        public LinksCascadeUniquenessAndDependenciesResolver(ILinks<T> links) : base(links) { }

        protected override T ResolveAddressChangeConflict(T oldLinkAddress, T newLinkAddress)
        {
            // TODO: Very similar to Merge (logic should be reused)
            ulong referencesAsSourceCount = (Integer<T>)Links.Count(Constants.Any, oldLinkAddress, Constants.Any);
            ulong referencesAsTargetCount = (Integer<T>)Links.Count(Constants.Any, Constants.Any, oldLinkAddress);

            var references = ArrayPool.Allocate<T>(referencesAsSourceCount + referencesAsTargetCount);

            var referencesFiller = new ArrayFiller<T>(references);

            Links.Each(oldLinkAddress, Constants.Any, referencesFiller.AddAndReturnTrue);
            Links.Each(Constants.Any, oldLinkAddress, referencesFiller.AddAndReturnTrue);

            for (ulong i = 0; i < referencesAsSourceCount; i++)
            {
                var reference = references[i];
                if (Equals(reference, oldLinkAddress)) continue;
                Links.Update(reference, newLinkAddress, Links.GetTarget(reference));
            }
            for (var i = (long)referencesAsSourceCount; i < references.Length; i++)
            {
                var reference = references[i];
                if (Equals(reference, oldLinkAddress)) continue;
                Links.Update(reference, Links.GetSource(reference), newLinkAddress);
            }

            ArrayPool.Free(references);

            return base.ResolveAddressChangeConflict(oldLinkAddress, newLinkAddress);
        }
    }
}
