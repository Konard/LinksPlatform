using System.Collections.Generic;

namespace Platform.Helpers
{
    public static class ListExtensions
    {
        public static bool AddAndReturnTrue<T>(this List<T> list, T element)
        {
            list.Add(element);
            return true;
        }
    }
}
