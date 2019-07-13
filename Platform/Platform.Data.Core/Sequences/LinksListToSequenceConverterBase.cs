using System.Collections.Generic;
using Platform.Interfaces;
using Platform.Data.Core.Doublets;

namespace Platform.Data.Core.Sequences
{
    public abstract class LinksListToSequenceConverterBase<TLink> : IConverter<IList<TLink>, TLink>
    {
        protected readonly ILinks<TLink> Links;

        public LinksListToSequenceConverterBase(ILinks<TLink> links)
        {
            Links = links;
        }

        public abstract TLink Convert(IList<TLink> source);
    }
}
