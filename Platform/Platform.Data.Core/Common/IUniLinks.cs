using System;
using System.Collections.Generic;

// ReSharper disable TypeParameterCanBeVariant

namespace Platform.Data.Core.Common
{
    /// <remarks>Minimal sufficient universal Links API (for bulk operations).</remarks>
    public partial interface IUniLinks<TLink>
    {
        IList<IList<IList<TLink>>> Trigger(IList<TLink> condition, IList<TLink> substitution);
    }

    /// <remarks>Minimal sufficient universal Links API (for step by step operations).</remarks>
    public partial interface IUniLinks<TLink>
    {
        /// <returns>
        /// TLink that represents True (was finished fully) or TLink that represents False (was stopped).
        /// This is done to assure ability to push up stop signal through recursion stack.
        /// </returns>
        TLink Trigger(IList<TLink> patternOrCondition, Func<IList<TLink>, TLink> matchHandler,
                      IList<TLink> substitution, Func<IList<TLink>, IList<TLink>, TLink> substitutionHandler);

        TLink Trigger(IList<TLink> restriction, Func<IList<TLink>, IList<TLink>, TLink> matchedHandler,
              IList<TLink> substitution, Func<IList<TLink>, IList<TLink>, TLink> substitutedHandler);

        // { 0, 0, 0 } => { itself, itself, itself } // create
        // { 1, any, any } => { itself, any, 3 } // update
        // { 3, any, any } => { 0, 0, 0 } // delete
    }

    /// <remarks>Extended with small optimization.</remarks>
    public partial interface IUniLinks<TLink>
    {
        /// <remarks>
        /// Something simple should be simple and optimized.
        /// </remarks>
        TLink Count(IList<TLink> restrictions);
    }

    /// <remarks>
    /// In/Out aliases for IUniLinks.
    /// TLink can be any number type of any size.
    /// </remarks>
    public interface IUniLinksIO<TLink>
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
        /// Possible interpretation
        /// * In(null, new[] { }) creates point (link that points to itself using minimum number of parts).
        /// * In(new[] { 4 }, null) deletes 4th link.
        /// * In(new[] { 4 }, new [] { 5 }) delete 5th link if it exists and moves 4th link to 5th index.
        /// * In(new[] { 4 }, new [] { 0, 2, 3 }) replaces 4th link with new pair link (with 2 as source and 3 as target), 0 means it can be placed in any address.
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