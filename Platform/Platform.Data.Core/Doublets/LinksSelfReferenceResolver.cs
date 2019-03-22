using System;
using System.Collections.Generic;

namespace Platform.Data.Core.Doublets
{
    public class LinksSelfReferenceResolver<T> : LinksDecoratorBase<T>
    {
        public LinksSelfReferenceResolver(ILinks<T> links) : base(links) { }

        public override T Each(Func<IList<T>, T> handler, IList<T> restrictions)
        {
            if (!Equals(Constants.Any, Constants.Itself) && (Equals(restrictions[Constants.SourcePart], Constants.Itself) || Equals(restrictions[Constants.TargetPart], Constants.Itself)))
                return Constants.Continue;

            return base.Each(handler, restrictions);
        }

        public override T Update(IList<T> restrictions)
        {
            restrictions[Constants.SourcePart] = Equals(restrictions[Constants.SourcePart], Constants.Itself) ? restrictions[Constants.IndexPart] : restrictions[Constants.SourcePart];
            restrictions[Constants.TargetPart] = Equals(restrictions[Constants.TargetPart], Constants.Itself) ? restrictions[Constants.IndexPart] : restrictions[Constants.TargetPart];

            return base.Update(restrictions);
        }
    }
}
