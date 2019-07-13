using System;
using System.Collections.Generic;

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
            for (var i = list.Count - 1; i >= 0; --i)
                if (!Equals(list[i], other[i]))
                    return false;
            return true;
        }

        public static T[] ToArray<T>(this IList<T> list, Func<T, bool> predicate)
        {
            if (list == null)
                return null;

            var count = 0;

            for (var i = 0; i < list.Count; i++)
                if (predicate(list[i]))
                    count++;

            var array = new T[count];

            if (count > 0)
            {
                count = 0;

                for (var i = 0; i < list.Count; i++)
                    if (predicate(list[i]))
                        array[count++] = list[i];
            }

            return array;
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
