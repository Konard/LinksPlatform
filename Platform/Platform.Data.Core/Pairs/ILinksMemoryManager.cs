using System;

namespace Platform.Data.Core.Pairs
{
    public interface ILinksMemoryManager<TLink>
    {
        ulong Total { get; }
        bool Exists(TLink link);
        ulong CalculateLinkTotalReferences(TLink link);
        bool Each(Func<TLink, bool> handler, params TLink[] valuesRestriction);
        void SetLinkValue(TLink link, params TLink[] values);
        TLink[] GetLinkValue(TLink link);
        TLink AllocateLink();
        void FreeLink(TLink link);
    }
}
