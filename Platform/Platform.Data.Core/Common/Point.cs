using System;
using System.Collections.Generic;

namespace Platform.Data.Core.Common
{
    public static class Point<TLink>
    {
        public static bool IsFullPoint(params TLink[] link) => IsFullPoint((IList<TLink>)link);

        public static bool IsFullPoint(IList<TLink> link)
        {
            if (link.Count <= 1)
                throw new ArgumentOutOfRangeException(nameof(link), "Cannot determine link's pointness using only its identifier.");

            var result = false;

            for (var i = 1; i < link.Count; i++)
            {
                result = Equals(link[0], link[i]);
                if (!result) break;
            }

            return result;
        }

        public static bool IsPartialPoint(params TLink[] link) => IsPartialPoint((IList<TLink>)link);

        public static bool IsPartialPoint(IList<TLink> link)
        {
            if (link.Count <= 1)
                throw new ArgumentOutOfRangeException(nameof(link), "Cannot determine link's pointness using only its identifier.");

            var result = false;

            for (var i = 1; i < link.Count; i++)
            {
                result = Equals(link[0], link[i]);
                if (result) break;
            }

            return result;
        }
    }
}
