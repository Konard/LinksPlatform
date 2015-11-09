using System;

namespace Platform.Data.Core
{
    /// <remarks>
    /// Minimal sufficient universal Links API.
    /// In stands for input.
    /// Out stands for output.
    /// TLink can be any number type of any size.
    /// </remarks>
    public interface IUniLinks<TLink>
    {
        /// <remarks>
        /// default(TLink) means any link.
        /// Single element pattern means just element (link).
        /// Handler gets array of link contents.
        /// * link[0] is index or identifier.
        /// * link[1] is source or first.
        /// * link[2] is target or second.
        /// * link[3] is linker or third.
        /// * link[n] is nth part/parent/element/value 
        /// of link (if variable length links used).
        /// 
        /// Stops and returns false if handler return false.
        /// 
        /// Acts as Each, Foreach, Select, Search, Match & ...
        /// 
        /// Handles all links in store if pattern/restrictions is not defined.
        /// </remarks>
        bool Out(Func<TLink[], bool> handler, params TLink[] pattern);

        /// <remarks>
        /// default(TLink) means itself.
        /// Equivalent to:
        /// * creation if before == null
        /// * deletion if after == null
        /// * update if before != null && after != null
        /// * default(TLink) if before == null && after == null
        /// 
        /// Possible interpretation of single element (array with length 1).
        /// * In(null, new[] { 0 }) creates point (link that points to itself).
        /// * In(new[] { 4 }, null) deletes link 4.
        /// * In(new[] { 4 }, new [] { 2, 3 }) replaces link 4 with new link
        /// (with 2 as source, and 3 as target).
        /// ...
        /// </remarks>
        TLink In(TLink[] before, TLink[] after);
    }

    public enum PartType : ulong
    {
        LinkIndexOrId = 0,
        LinkSourceOrFirst = 1,
        LinkTargetOrSecond = 2,
        LinkLinkerOrThird = 3
    }

    /// <remarks>Contains some optimizations of Out.</remarks>
    public interface IUniLinksWithOutEachExtensions<TLink> : IUniLinks<TLink>
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

    /// <remarks>
    /// Get/Set aliases for IUniLinks.
    /// </remarks>
    public interface IUniLinksGS<TLink>
    {
        TLink Get(ulong partType, TLink link);
        bool Get(Func<TLink, bool> handler, params TLink[] pattern);
        TLink Set(TLink[] before, TLink[] after);
    }

    /// <remarks>
    /// Read/Write aliases for IUniLinks.
    /// </remarks>
    public interface IUniLinksRW<TLink>
    {
        TLink Read(ulong partType, TLink link);
        bool Read(Func<TLink, bool> handler, params TLink[] pattern);
        TLink Write(TLink[] before, TLink[] after);
    }

    /// <remarks>
    /// CRUD aliases for IUniLinks.
    /// </remarks>
    public interface IUniLinksCRUD<TLink>
    {
        TLink Read(ulong partType, TLink link);
        bool Read(Func<TLink, bool> handler, params TLink[] pattern);
        TLink Create(TLink[] parts);
        TLink Update(TLink[] before, TLink[] after);
        void Delete(TLink[] parts);
    }
}
