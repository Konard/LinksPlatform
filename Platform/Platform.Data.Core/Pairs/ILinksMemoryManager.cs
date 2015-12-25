using System;

namespace Platform.Data.Core.Pairs
{
    public interface ILinksMemoryManager<TLink>
    {
        ulong Count(params TLink[] restrictions);
        bool Each(Func<TLink, bool> handler, params TLink[] restrictions);
        void SetLinkValue(params TLink[] parts);
        TLink[] GetLinkValue(TLink link);
        TLink AllocateLink();
        void FreeLink(TLink link);
    }
}
