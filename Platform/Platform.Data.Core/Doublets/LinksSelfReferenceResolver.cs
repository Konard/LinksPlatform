using System;
using System.Collections.Generic;

namespace Platform.Data.Core.Doublets
{
    public class LinksSelfReferenceResolver<TLink> : LinksDecoratorBase<TLink>
    {
        private static readonly EqualityComparer<TLink> EqualityComparer = EqualityComparer<TLink>.Default;

        public LinksSelfReferenceResolver(ILinks<TLink> links) : base(links) { }

        public override TLink Each(Func<IList<TLink>, TLink> handler, IList<TLink> restrictions)
        {
            if (!EqualityComparer.Equals(Constants.Any, Constants.Itself)
                && ((restrictions.Count > Constants.IndexPart && EqualityComparer.Equals(restrictions[Constants.IndexPart], Constants.Itself))
                 || (restrictions.Count > Constants.SourcePart && EqualityComparer.Equals(restrictions[Constants.SourcePart], Constants.Itself))
                 || (restrictions.Count > Constants.TargetPart && EqualityComparer.Equals(restrictions[Constants.TargetPart], Constants.Itself))))
                return Constants.Continue;

            return base.Each(handler, restrictions);
        }

        public override TLink Update(IList<TLink> restrictions)
        {
            restrictions[Constants.SourcePart] = EqualityComparer.Equals(restrictions[Constants.SourcePart], Constants.Itself) ? restrictions[Constants.IndexPart] : restrictions[Constants.SourcePart];
            restrictions[Constants.TargetPart] = EqualityComparer.Equals(restrictions[Constants.TargetPart], Constants.Itself) ? restrictions[Constants.IndexPart] : restrictions[Constants.TargetPart];

            return base.Update(restrictions);
        }
    }
}
