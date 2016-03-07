using System;

namespace Platform.Data.Core
{
    public static class Point<TLink>
    {
        public static bool IsFullPoint(params TLink[] link)
        {
            if (link.Length <= 1)
                throw new ArgumentOutOfRangeException("link", "Cannot determine link's pointness using only its identifier.");

            var result = false;

            for (int i = 1; i < link.Length; i++)
            {
                result = Equals(link[0], link[i]);
                if (!result) break;
            }

            return result;
        }

        public static bool IsPartialPoint(params TLink[] link)
        {
            if (link.Length <= 1)
                throw new ArgumentOutOfRangeException("link", "Cannot determine link's pointness using only its identifier.");

            var result = false;

            for (int i = 1; i < link.Length; i++)
            {
                result = Equals(link[0], link[i]);
                if (result) break;
            }

            return result;
        }
    }
}
