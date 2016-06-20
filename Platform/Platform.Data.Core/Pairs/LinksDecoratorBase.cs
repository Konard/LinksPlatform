using System;
using System.Collections.Generic;

namespace Platform.Data.Core.Pairs
{
    public abstract class LinksDecoratorBase<T> : ILinks<T>
    {
        public ILinksCombinedConstants<bool, T, int> Constants { get; }

        public readonly ILinks<T> Links;

        protected LinksDecoratorBase(ILinks<T> links)
        {
            Links = links;
            Constants = links.Constants;
        }

        public virtual T Count(params T[] restrictions) => Links.Count(restrictions);

        public virtual bool Each(Func<IList<T>, bool> handler, IList<T> restrictions) => Links.Each(handler, restrictions);

        public virtual T Create() => Links.Create();

        public virtual T Update(IList<T> restrictions) => Links.Update(restrictions);

        public virtual void Delete(T link) => Links.Delete(link);
    }
}
