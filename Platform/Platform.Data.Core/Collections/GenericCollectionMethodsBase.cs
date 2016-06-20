using System;
using System.Runtime.CompilerServices;
using Platform.Helpers;

namespace Platform.Data.Core.Collections
{
    public abstract class GenericCollectionMethodsBase<TElement>
    {
        private static readonly TElement Zero = default(TElement);
        private static readonly TElement One = MathHelpers.Increment(Zero);
        private static readonly TElement Two = MathHelpers.Increment(One);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual TElement GetZero() => Zero;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual TElement GetOne() => One;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual TElement GetTwo() => Two;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool ValueEqualToZero(IntPtr pointer) => MathHelpers.Equals(pointer.GetValue<TElement>(), GetZero());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool EqualToZero(TElement value) => MathHelpers.Equals(value, GetZero());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool Equals(TElement first, TElement second) => MathHelpers.Equals(first, second);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool GreaterThanZero(TElement value) => MathHelpers.GreaterThan(value, GetZero());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool GreaterThan(TElement first, TElement second) => MathHelpers.GreaterThan(first, second);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool GreaterOrEqualThanZero(TElement value) => GreaterOrEqualThan(value, GetZero());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool GreaterOrEqualThan(TElement first, TElement second) => MathHelpers.GreaterOrEqualThan(first, second);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool LessOrEqualThanZero(TElement value) => LessOrEqualThan(value, GetZero());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool LessOrEqualThan(TElement first, TElement second) => MathHelpers.LessOrEqualThan(first, second);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool LessThanZero(TElement value) => MathHelpers.LessThan(value, GetZero());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool LessThan(TElement first, TElement second) => MathHelpers.LessThan(first, second);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual TElement Increment(TElement value) => MathHelpers.Increment(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual TElement Decrement(TElement value) => MathHelpers.Decrement(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual TElement Add(TElement first, TElement second) => MathHelpers.Add(first, second);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual TElement Subtract(TElement first, TElement second) => MathHelpers.Subtract(first, second);
    }
}
