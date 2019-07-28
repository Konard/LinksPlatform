using System;
using System.Collections.Generic;

// ReSharper disable TypeParameterCanBeVariant

namespace Platform.Data.Universal
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
        /// <remarks>
        /// { 0, 0, 0 } => { itself, itself, itself } // create
        /// { 1, any, any } => { itself, any, 3 } // update
        /// { 3, any, any } => { 0, 0, 0 } // delete
        /// </remarks>
        TLink Trigger(IList<TLink> patternOrCondition, Func<IList<TLink>, TLink> matchHandler,
                      IList<TLink> substitution, Func<IList<TLink>, IList<TLink>, TLink> substitutionHandler);

        TLink Trigger(IList<TLink> restriction, Func<IList<TLink>, IList<TLink>, TLink> matchedHandler,
              IList<TLink> substitution, Func<IList<TLink>, IList<TLink>, TLink> substitutedHandler);
    }

    /// <remarks>Extended with small optimization.</remarks>
    public partial interface IUniLinks<TLink>
    {
        /// <remarks>
        /// Something simple should be simple and optimized.
        /// </remarks>
        TLink Count(IList<TLink> restrictions);
    }
}