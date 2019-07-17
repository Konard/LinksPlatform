using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Platform.Helpers.Numbers;

namespace Platform.Data.Core.Sequences.Frequencies.Cache
{
    /// <remarks>
    /// TODO: Может стоит попробовать ref во всех методах (IRefEqualityComparer)
    /// 2x faster with comparer 
    /// </remarks>
    public class DoubletComparer<T> : IEqualityComparer<Doublet<T>>
    {
        private static readonly EqualityComparer<T> EqualityComparer = EqualityComparer<T>.Default;

        public static readonly DoubletComparer<T> Default = new DoubletComparer<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Doublet<T> x, Doublet<T> y) => EqualityComparer.Equals(x.Source, y.Source) && EqualityComparer.Equals(x.Target, y.Target);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetHashCode(Doublet<T> obj) => unchecked((obj.Source.GetHashCode() << 15) ^ obj.Target.GetHashCode());
    }
}
