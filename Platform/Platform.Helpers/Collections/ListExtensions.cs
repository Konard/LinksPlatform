using System.Collections.Generic;

namespace Platform.Helpers.Collections
{
    public static class ListExtensions
    {
        public static bool AddAndReturnTrue<T>(this IList<T> list, T element)
        {
            list.Add(element);
            return true;
        }

        public static bool IsNullOrEmpty<T>(this IList<T> list)
        {
            return list == null || list.Count == 0;
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
            var comparer = EqualityComparer<T>.Default;
            for (var i = list.Count - 1; i >= 0; --i)
                if (!comparer.Equals(list[i], other[i]))
                    return false;
            return true;
        }
    }
}
