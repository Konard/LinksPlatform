using System;
using System.Collections.Generic;

namespace Platform.Helpers.Collections
{
    /// <remarks>
    /// Based on http://stackoverflow.com/questions/14663168/an-integer-array-as-a-key-for-dictionary
    /// </remarks>
    public class ArrayComparer<T> : IEqualityComparer<T[]>
        where T : IEquatable<T>
    {
        public bool Equals(T[] x, T[] y) => x.EqualTo(y);

        /// <remarks>
        /// Based on http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
        /// </remarks>
        public int GetHashCode(T[] obj)
        {
            var result = 17;
            for (var i = 0; i < obj.Length; i++)
                unchecked { result = result * 23 + obj[i].GetHashCode(); }
            return result;
        }
    }
}
