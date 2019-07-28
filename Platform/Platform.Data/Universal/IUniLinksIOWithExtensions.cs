// ReSharper disable TypeParameterCanBeVariant

namespace Platform.Data.Core.Common
{
    /// <remarks>Contains some optimizations of Out.</remarks>
    public interface IUniLinksIOWithExtensions<TLink> : IUniLinksIO<TLink>
    {
        /// <remarks>
        /// default(TLink) means nothing or null.
        /// Single element pattern means just element (link).
        /// OutPart(n, null) returns default(TLink).
        /// OutPart(0, pattern) ~ Exists(link) or Search(pattern)
        /// OutPart(1, pattern) ~ GetSource(link) or GetSource(Search(pattern))
        /// OutPart(2, pattern) ~ GetTarget(link) or GetTarget(Search(pattern))
        /// OutPart(3, pattern) ~ GetLinker(link) or GetLinker(Search(pattern))
        /// OutPart(n, pattern) => For any variable length links, returns link or default(TLink).
        /// 
        /// Outs(returns) inner contents of link, its part/parent/element/value.
        /// </remarks>
        TLink OutOne(ulong partType, params TLink[] pattern);

        /// <remarks>OutCount() returns total links in store as array.</remarks>
        TLink[][] OutAll(params TLink[] pattern);

        /// <remarks>OutCount() returns total amount of links in store.</remarks>
        ulong OutCount(params TLink[] pattern);
    }
}