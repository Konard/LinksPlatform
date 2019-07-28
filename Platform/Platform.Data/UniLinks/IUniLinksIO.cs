using System;

// ReSharper disable TypeParameterCanBeVariant

namespace Platform.Data.Core.Common
{
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
        /// * In(new[] { 4 }, new [] { 0, 2, 3 }) replaces 4th link with new doublet link (with 2 as source and 3 as target), 0 means it can be placed in any address.
        /// ...
        /// </remarks>
        TLink In(TLink[] before, TLink[] after);
    }
}