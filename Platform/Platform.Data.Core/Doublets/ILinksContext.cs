using System;
using Platform.Memory;

namespace Platform.Data.Core.Doublets
{
    internal interface ILinksContext<TLink, TConstants, TMemory, TLinks> : IDisposable
        where TMemory : IMemory
        where TLinks : ILinks<TLink>
    {
        TConstants Constants { get; }
        TMemory Memory { get; }
        TLinks Links { get; }
    }
}
