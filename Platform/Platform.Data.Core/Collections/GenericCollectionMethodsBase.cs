using System;
using System.Runtime.CompilerServices;
using Platform.Helpers;
using Platform.Helpers.Unsafe;

namespace Platform.Data.Core.Collections
{
    public abstract class GenericCollectionMethodsBase<TElement>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual TElement GetZero() => Integer<TElement>.Zero;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual TElement GetOne() => Integer<TElement>.One;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual TElement GetTwo() => Integer<TElement>.Two;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool ValueEqualToZero(IntPtr pointer) => MathHelpers<TElement>.IsEquals(pointer.GetValue<TElement>(), GetZero());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool EqualToZero(TElement value) => MathHelpers<TElement>.IsEquals(value, GetZero());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool IsEquals(TElement first, TElement second) => MathHelpers<TElement>.IsEquals(first, second);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool GreaterThanZero(TElement value) => MathHelpers<TElement>.GreaterThan(value, GetZero());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool GreaterThan(TElement first, TElement second) => MathHelpers<TElement>.GreaterThan(first, second);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool GreaterOrEqualThanZero(TElement value) => MathHelpers<TElement>.GreaterOrEqualThan(value, GetZero());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool GreaterOrEqualThan(TElement first, TElement second) => MathHelpers<TElement>.GreaterOrEqualThan(first, second);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool LessOrEqualThanZero(TElement value) => MathHelpers<TElement>.LessOrEqualThan(value, GetZero());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool LessOrEqualThan(TElement first, TElement second) => MathHelpers<TElement>.LessOrEqualThan(first, second);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool LessThanZero(TElement value) => MathHelpers<TElement>.LessThan(value, GetZero());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool LessThan(TElement first, TElement second) => MathHelpers<TElement>.LessThan(first, second);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual TElement Increment(TElement value) => MathHelpers<TElement>.Increment(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual TElement Decrement(TElement value) => MathHelpers<TElement>.Decrement(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual TElement Add(TElement first, TElement second) => MathHelpers<TElement>.Add(first, second);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual TElement Subtract(TElement first, TElement second) => MathHelpers<TElement>.Subtract(first, second);
    }
}
