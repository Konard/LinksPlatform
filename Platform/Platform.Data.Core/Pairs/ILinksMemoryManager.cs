using System;

namespace Platform.Data.Core.Pairs
{
    // TODO: Decide between IList and Array (or make both and compare)
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
