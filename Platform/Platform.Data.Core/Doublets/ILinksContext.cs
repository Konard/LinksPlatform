using System;
using Platform.Memory;

namespace Platform.Data.Core.Pairs
{
    public interface ILinksContext<TLink, TConstants, TMemory, TManager, TLinks> : IDisposable
        where TMemory : IMemory
        where TManager : ILinksMemoryManager<TLink>
        where TLinks : ILinks<TLink>
    {
        TConstants Constants { get; }
        TMemory Memory { get; }
        TManager MemoryManager { get; }
        TLinks Links { get; }
    }
}
