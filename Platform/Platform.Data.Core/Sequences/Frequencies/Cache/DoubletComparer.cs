using Platform.Helpers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Platform.Data.Core.Sequences.Frequencies.Cache
{
    /// <remarks>
    /// TODO: Может стоит попробовать ref во всех методах (IRefEqualityComparer)
    /// 2x faster with comparer 
    /// </remarks>
    public class DoubletComparer<T> : IEqualityComparer<Doublet<T>>
    {
        public static readonly DoubletComparer<T> Default = new DoubletComparer<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Doublet<T> x, Doublet<T> y) => MathHelpers<T>.IsEquals(x.Source, y.Source) && MathHelpers<T>.IsEquals(x.Target, y.Target);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetHashCode(Doublet<T> obj) => unchecked((obj.Source.GetHashCode() << 15) ^ obj.Target.GetHashCode());
    }
}
