using System;
using System.Collections;

namespace Platform.Helpers
{
    public class Comparer : IComparer
    {
        private readonly Func<object, object, int> _compare;

        public Comparer(IComparer comparer)
            : this(comparer.Compare)
        {
        }

        public Comparer(Func<object, object, int> compare) => _compare = compare;

        public int Compare(object x, object y) => _compare(x, y);

        public static implicit operator Comparer(Func<object, object, int> compare) => new Comparer(compare);

        public static implicit operator Func<object, object, int>(Comparer comparer) => comparer._compare;
    }
}
