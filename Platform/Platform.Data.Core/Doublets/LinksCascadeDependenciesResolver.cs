using System.Collections.Generic;
using Platform.Helpers.Collections.Arrays;
using Platform.Helpers.Numbers;

namespace Platform.Data.Core.Doublets
{
    public class LinksCascadeDependenciesResolver<TLink> : LinksDecoratorBase<TLink>
    {
        private static readonly EqualityComparer<TLink> EqualityComparer = EqualityComparer<TLink>.Default;

        public LinksCascadeDependenciesResolver(ILinks<TLink> links) : base(links) {}

        public override void Delete(TLink link)
        {
            EnsureNoDependenciesOnDelete(link);
            base.Delete(link);
        }

        public void EnsureNoDependenciesOnDelete(TLink link)
        {
            ulong referencesCount = (Integer<TLink>)Links.Count(Constants.Any, link);

            var references = ArrayPool.Allocate<TLink>(referencesCount);

            var referencesFiller = new ArrayFiller<TLink, TLink>(references, Constants.Continue);

            Links.Each(referencesFiller.AddFirstAndReturnConstant, Constants.Any, link);

            //references.Sort(); // TODO: Решить необходимо ли для корректного порядка отмены операций в транзакциях

            for (var i = (long)referencesCount - 1; i >= 0; i--)
            {
                if (EqualityComparer.Equals(references[i], link)) continue;
                Links.Delete(references[i]);
            }

            ArrayPool.Free(references);
        }
    }
}
