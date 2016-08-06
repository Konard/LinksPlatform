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
        protected virtual bool ValueEqualToZero(IntPtr pointer) => MathHelpers<TElement>.CompiledOperations.Equals(pointer.GetValue<TElement>(), GetZero());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool EqualToZero(TElement value) => MathHelpers<TElement>.CompiledOperations.Equals(value, GetZero());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool Equals(TElement first, TElement second) => MathHelpers<TElement>.CompiledOperations.Equals(first, second);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool GreaterThanZero(TElement value) => MathHelpers<TElement>.CompiledOperations.Greater(value, GetZero());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool GreaterThan(TElement first, TElement second) => MathHelpers<TElement>.CompiledOperations.Greater(first, second);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool GreaterOrEqualThanZero(TElement value) => MathHelpers<TElement>.CompiledOperations.GreaterOrEqual(value, GetZero()); // MathHelpers.GreaterOrEqualThan(value, GetZero());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool GreaterOrEqualThan(TElement first, TElement second) => MathHelpers<TElement>.CompiledOperations.GreaterOrEqual(first, second); // MathHelpers.GreaterOrEqualThan(first, second);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool LessOrEqualThanZero(TElement value) => MathHelpers<TElement>.CompiledOperations.LessOrEqual(value, GetZero()); // MathHelpers.LessOrEqualThan(value, GetZero());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool LessOrEqualThan(TElement first, TElement second) => MathHelpers<TElement>.CompiledOperations.LessOrEqual(first, second); // MathHelpers.LessOrEqualThan(first, second);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool LessThanZero(TElement value) => MathHelpers<TElement>.CompiledOperations.Less(value, GetZero());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool LessThan(TElement first, TElement second) => MathHelpers<TElement>.CompiledOperations.Less(first, second);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual TElement Increment(TElement value) => MathHelpers<TElement>.CompiledOperations.Increment(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual TElement Decrement(TElement value) => MathHelpers<TElement>.CompiledOperations.Decrement(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual TElement Add(TElement first, TElement second) => MathHelpers<TElement>.CompiledOperations.Add(first, second);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual TElement Subtract(TElement first, TElement second) => MathHelpers<TElement>.CompiledOperations.Subtract(first, second);
    }
}
