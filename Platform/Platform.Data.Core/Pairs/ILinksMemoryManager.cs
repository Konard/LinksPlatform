using System;

namespace Platform.Data.Core.Pairs
{
    public interface ILinksMemoryManager<TLink>
    {
        bool Exists(TLink link);
        ulong Count(params TLink[] restrictions);
        bool Each(Func<TLink, bool> handler, params TLink[] valuesRestriction);
        void SetLinkValue(TLink link, params TLink[] values);
        TLink[] GetLinkValue(TLink link);
        TLink AllocateLink();
        void FreeLink(TLink link);
    }
}
