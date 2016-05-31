using System.Collections.Generic;

namespace Platform.Helpers.Collections
{
    public static class SetExtensions
    {
        public static void AddAndReturnVoid<T>(this ISet<T> set, T element) => set.Add(element);
    }
}
