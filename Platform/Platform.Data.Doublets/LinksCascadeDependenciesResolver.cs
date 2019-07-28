using System.Collections.Generic;
using Platform.Collections.Arrays;
using Platform.Numbers;

namespace Platform.Data.Doublets
{
    public class LinksCascadeDependenciesResolver<TLink> : LinksDecoratorBase<TLink>
    {
        private static readonly EqualityComparer<TLink> _equalityComparer = EqualityComparer<TLink>.Default;

        public LinksCascadeDependenciesResolver(ILinks<TLink> links) : base(links) { }

        public override void Delete(TLink link)
        {
            EnsureNoDependenciesOnDelete(link);
            base.Delete(link);
        }

        public void EnsureNoDependenciesOnDelete(TLink link)
        {
            ulong referencesCount = (Integer<TLink>)Links.Count(Constants.Any, link);

            var references = ArrayPool.Allocate<TLink>((long)referencesCount);

            var referencesFiller = new ArrayFiller<TLink, TLink>(references, Constants.Continue);

            Links.Each(referencesFiller.AddFirstAndReturnConstant, Constants.Any, link);

            //references.Sort(); // TODO: Решить необходимо ли для корректного порядка отмены операций в транзакциях

            for (var i = (long)referencesCount - 1; i >= 0; i--)
            {
                if (_equalityComparer.Equals(references[i], link))
                {
                    continue;
                }

                Links.Delete(references[i]);
            }

            ArrayPool.Free(references);
        }
    }
}
