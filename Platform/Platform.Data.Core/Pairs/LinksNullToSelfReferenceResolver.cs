﻿using System.Collections.Generic;

namespace Platform.Data.Core.Pairs
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
            restrictions[Constants.SourcePart] = Equals(restrictions[Constants.SourcePart], Constants.Null) ? restrictions[Constants.IndexPart] : restrictions[Constants.SourcePart];
            restrictions[Constants.TargetPart] = Equals(restrictions[Constants.TargetPart], Constants.Null) ? restrictions[Constants.IndexPart] : restrictions[Constants.TargetPart];

            return base.Update(restrictions);
        }

        public override void Delete(T link)
        {
            // TODO: Looks like this can be moved/copied to separate layer
            Links.Update(link, Constants.Null, Constants.Null);
            base.Delete(link);
        }
    }
}