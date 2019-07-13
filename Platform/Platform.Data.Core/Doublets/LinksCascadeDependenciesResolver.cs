using Platform.Helpers;
using Platform.Helpers.Collections.Array;

namespace Platform.Data.Core.Doublets
{
    public class LinksCascadeDependenciesResolver<T> : LinksDecoratorBase<T>
    {
        public LinksCascadeDependenciesResolver(ILinks<T> links) : base(links) {}

        public override void Delete(T link)
        {
            EnsureNoDependenciesOnDelete(link);
            base.Delete(link);
        }

        public void EnsureNoDependenciesOnDelete(T link)
        {
            ulong referencesCount = (Integer<T>)Links.Count(Constants.Any, link);

            var references = ArrayPool.Allocate<T>(referencesCount);

            var referencesFiller = new ArrayFiller<T, T>(references, Constants.Continue);

            Links.Each(referencesFiller.AddFirstAndReturnConstant, Constants.Any, link);

            //references.Sort(); // TODO: Решить необходимо ли для корректного порядка отмены операций в транзакциях

            for (var i = (long)referencesCount - 1; i >= 0; i--)
            {
                if (MathHelpers<T>.IsEquals(references[i], link)) continue;
                Links.Delete(references[i]);
            }

            ArrayPool.Free(references);
        }
    }
}
