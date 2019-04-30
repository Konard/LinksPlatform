using System.Collections.Generic;
using Platform.Helpers;

namespace Platform.Data.Core.Doublets
{
    public class LinksNullToSelfReferenceResolver<T> : LinksDecoratorBase<T>
    {
        public LinksNullToSelfReferenceResolver(ILinks<T> links) : base(links) {}

        public override T Create()
        {
            var link = base.Create();
            return Links.Update(link, link, link);
        }

        public override T Update(IList<T> restrictions)
        {
            restrictions[Constants.SourcePart] = MathHelpers<T>.IsEquals(restrictions[Constants.SourcePart], Constants.Null) ? restrictions[Constants.IndexPart] : restrictions[Constants.SourcePart];
            restrictions[Constants.TargetPart] = MathHelpers<T>.IsEquals(restrictions[Constants.TargetPart], Constants.Null) ? restrictions[Constants.IndexPart] : restrictions[Constants.TargetPart];

            return base.Update(restrictions);
        }
    }
}
