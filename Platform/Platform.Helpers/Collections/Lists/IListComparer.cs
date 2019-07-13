using System.Collections.Generic;

namespace Platform.Helpers.Collections.Lists
{
    public class IListComparer<T> : IComparer<IList<T>>
    {
        public int Compare(IList<T> left, IList<T> right) => left.CompareTo(right);
    }
}
