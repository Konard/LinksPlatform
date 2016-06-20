using System;
using System.Runtime.CompilerServices;
using Platform.Helpers;

namespace Platform.Data.Core.Collections.Trees
{
    public abstract class SizeBalancedTreeMethodsBase<TElement> : GenericCollectionMethodsBase<TElement>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract IntPtr GetLeft(TElement node);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract IntPtr GetRight(TElement node);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract TElement GetSize(TElement node);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void SetLeft(TElement node, TElement left);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void SetRight(TElement node, TElement right);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void SetSize(TElement node, TElement size);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract bool FirstIsToTheLeftOfSecond(TElement first, TElement second);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract bool FirstIsToTheRightOfSecond(TElement first, TElement second);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void IncrementSize(TElement node) => SetSize(node, Increment(GetSize(node)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DecrementSize(TElement node) => SetSize(node, Decrement(GetSize(node)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected TElement GetLeftSize(TElement node) => GetSizeOrZero(GetLeft(node).GetValue<TElement>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected TElement GetRightSize(TElement node) => GetSizeOrZero(GetRight(node).GetValue<TElement>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected TElement GetSizeOrZero(TElement node) => EqualToZero(node) ? GetZero() : GetSize(node);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void FixSize(TElement node) => SetSize(node, Increment(Add(GetLeftSize(node), GetRightSize(node))));

        protected void LeftRotate(IntPtr root)
        {
            var rootValue = root.GetValue<TElement>();
            var right = GetRight(rootValue).GetValue<TElement>();
            if (EqualToZero(right))
                return;
            SetRight(rootValue, GetLeft(right).GetValue<TElement>());
            SetLeft(right, rootValue);
            SetSize(right, GetSize(rootValue));
            FixSize(rootValue);
            root.SetValue(right);
        }

        protected void RightRotate(IntPtr root)
        {
            var rootValue = root.GetValue<TElement>();
            var left = GetLeft(rootValue).GetValue<TElement>();
            if (EqualToZero(left))
                return;
            SetLeft(rootValue, GetRight(left).GetValue<TElement>());
            SetRight(left, rootValue);
            SetSize(left, GetSize(rootValue));
            FixSize(rootValue);
            root.SetValue(left);
        }

        public bool Contains(TElement node, TElement root)
        {
            while (!EqualToZero(root))
            {
                if (FirstIsToTheLeftOfSecond(node, root)) // node.Key < root.Key
                    root = GetLeft(root).GetValue<TElement>();
                else if (FirstIsToTheRightOfSecond(node, root)) // node.Key > root.Key
                    root = GetRight(root).GetValue<TElement>();
                else // node.Key == root.Key
                    return true;
            }
            return false;
        }
    }
}
