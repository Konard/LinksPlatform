using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Platform.Helpers.Collections;
using Platform.Helpers.Numbers;

namespace Platform.Data.Core.Common
{
    public static class Point<TLink>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFullPoint(params TLink[] link) => IsFullPoint((IList<TLink>)link);

        public static bool IsFullPoint(IList<TLink> link)
        {
            if (link.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(link));
            if (link.Count <= 1)
                throw new ArgumentOutOfRangeException(nameof(link), "Cannot determine link's pointness using only its identifier.");
            return IsFullPointUnchecked(link);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFullPointUnchecked(IList<TLink> link)
        {
            var result = true;
            for (var i = 1; result && i < link.Count; i++)
                result = MathHelpers<TLink>.IsEquals(link[0], link[i]);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPartialPoint(params TLink[] link) => IsPartialPoint((IList<TLink>)link);

        public static bool IsPartialPoint(IList<TLink> link)
        {
            if (link.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(link));
            if (link.Count <= 1)
                throw new ArgumentOutOfRangeException(nameof(link), "Cannot determine link's pointness using only its identifier.");
            return IsPartialPointUnchecked(link);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPartialPointUnchecked(IList<TLink> link)
        {
            var result = false;
            for (var i = 1; !result && i < link.Count; i++)
                result = MathHelpers<TLink>.IsEquals(link[0], link[i]);
            return result;
        }
    }
}
