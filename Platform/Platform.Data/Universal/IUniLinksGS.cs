using System;

// ReSharper disable TypeParameterCanBeVariant

namespace Platform.Data.Core.Common
{
    /// <remarks>
    /// Get/Set aliases for IUniLinks.
    /// </remarks>
    public interface IUniLinksGS<TLink>
    {
        TLink Get(ulong partType, TLink link);
        bool Get(Func<TLink, bool> handler, params TLink[] pattern);
        TLink Set(TLink[] before, TLink[] after);
    }
}