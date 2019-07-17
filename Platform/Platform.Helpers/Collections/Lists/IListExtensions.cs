using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.Helpers.Collections.Lists
{
    public static class ListExtensions
    {
        public static bool AddAndReturnTrue<T>(this IList<T> list, T element)
        {
            list.Add(element);
            return true;
        }

        public static bool EqualTo<T>(this IList<T> list, IList<T> other)
        {
            if (ReferenceEquals(list, other))
                return true;
            if (list.IsNullOrEmpty() && other.IsNullOrEmpty())
                return true;
            if (list.IsNullOrEmpty() || other.IsNullOrEmpty())
                return false;
            if (list.Count != other.Count)
                return false;
            var equalityComparer = EqualityComparer<T>.Default;
            for (var i = list.Count - 1; i >= 0; --i)
                if (!equalityComparer.Equals(list[i], other[i]))
                    return false;
            return true;
        }

        public static T[] ToArray<T>(this IList<T> list, Func<T, bool> predicate)
        {
            if (list == null)
                return null;

            var result = new List<T>(list.Count);

            for (var i = 0; i < list.Count; i++)
                if (predicate(list[i]))
                    result.Add(list[i]);

            return result.ToArray();
        }

        public static void ForEach<T>(this IList<T> list, Action<T> action)
        {
            for (var i = 0; i < list.Count; i++)
                action(list[i]);
        }

        /// <remarks>
        /// Based on http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
        /// </remarks>
        public static int GenerateHashCode<T>(this IList<T> list)
        {
            var result = 17;
            for (var i = 0; i < list.Count; i++)
                result = unchecked(result * 23 + list[i].GetHashCode());
            return result;
        }

        public static int CompareTo<T>(this IList<T> left, IList<T> right)
        {
            var comparer = Comparer<T>.Default;
            var leftLength = left?.Count ?? 0;
            var rightLength = right?.Count ?? 0;
            var intermediateResult = leftLength.CompareTo(rightLength);
            for (var i = 0; intermediateResult == 0 && i < leftLength; i++)
                intermediateResult = comparer.Compare(left[i], right[i]);
            return intermediateResult;
        }
    }
}
