using System.Collections.Generic;

namespace Platform.Helpers.Collections.Lists
{
    public class IListEqualityComparer<T> : IEqualityComparer<IList<T>>
    {
        public bool Equals(IList<T> left, IList<T> right) => left.EqualTo(right);

        public int GetHashCode(IList<T> list) => list.GenerateHashCode();
    }
}
