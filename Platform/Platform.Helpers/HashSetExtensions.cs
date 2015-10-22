using System.Collections.Generic;

namespace Platform.Helpers
{
    public static class HashSetExtensions
    {
        public static void AddAndReturnVoid<T>(this HashSet<T> set, T element)
        {
            set.Add(element);
        }
    }
}
