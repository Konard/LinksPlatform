using System;
using System.Collections.Generic;
using Platform.Helpers;

namespace Platform.Data.Core.Doublets
{
    public class LinksSelfReferenceResolver<T> : LinksDecoratorBase<T>
    {
        public LinksSelfReferenceResolver(ILinks<T> links) : base(links) { }

        public override T Each(Func<IList<T>, T> handler, IList<T> restrictions)
        {
            if (!MathHelpers<T>.IsEquals(Constants.Any, Constants.Itself) && (MathHelpers<T>.IsEquals(restrictions[Constants.SourcePart], Constants.Itself) || MathHelpers<T>.IsEquals(restrictions[Constants.TargetPart], Constants.Itself)))
                return Constants.Continue;

            return base.Each(handler, restrictions);
        }

        public override T Update(IList<T> restrictions)
        {
            restrictions[Constants.SourcePart] = MathHelpers<T>.IsEquals(restrictions[Constants.SourcePart], Constants.Itself) ? restrictions[Constants.IndexPart] : restrictions[Constants.SourcePart];
            restrictions[Constants.TargetPart] = MathHelpers<T>.IsEquals(restrictions[Constants.TargetPart], Constants.Itself) ? restrictions[Constants.IndexPart] : restrictions[Constants.TargetPart];

            return base.Update(restrictions);
        }
    }
}
