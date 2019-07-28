using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Platform.Exceptions;
using Platform.Collections;

namespace Platform.Data.Core.Common
{
    public static class Point<TLink>
    {
        private static readonly EqualityComparer<TLink> _equalityComparer = EqualityComparer<TLink>.Default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFullPoint(params TLink[] link) => IsFullPoint((IList<TLink>)link);

        public static bool IsFullPoint(IList<TLink> link)
        {
            Ensure.Always.ArgumentNotEmpty(link, nameof(link));
            if (link.Count <= 1)
            {
                throw new ArgumentOutOfRangeException(nameof(link), "Cannot determine link's pointness using only its identifier.");
            }
            return IsFullPointUnchecked(link);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFullPointUnchecked(IList<TLink> link)
        {
            var result = true;
            for (var i = 1; result && i < link.Count; i++)
            {
                result = _equalityComparer.Equals(link[0], link[i]);
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPartialPoint(params TLink[] link) => IsPartialPoint((IList<TLink>)link);

        public static bool IsPartialPoint(IList<TLink> link)
        {
            Ensure.Always.ArgumentNotEmpty(link, nameof(link));
            if (link.Count <= 1)
            {
                throw new ArgumentOutOfRangeException(nameof(link), "Cannot determine link's pointness using only its identifier.");
            }
            return IsPartialPointUnchecked(link);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPartialPointUnchecked(IList<TLink> link)
        {
            var result = false;
            for (var i = 1; !result && i < link.Count; i++)
            {
                result = _equalityComparer.Equals(link[0], link[i]);
            }
            return result;
        }
    }
}
