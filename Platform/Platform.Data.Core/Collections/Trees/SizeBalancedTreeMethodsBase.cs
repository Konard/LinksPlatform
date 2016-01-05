using System.Runtime.CompilerServices;

namespace Platform.Data.Core.Collections.Trees
{
    public abstract unsafe class SizeBalancedTreeMethodsBase
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract ulong* GetLeft(ulong node);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract ulong* GetRight(ulong node);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract ulong GetSize(ulong node);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void SetLeft(ulong node, ulong left);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void SetRight(ulong node, ulong right);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void SetSize(ulong node, ulong size);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract bool FirstIsToTheLeftOfSecond(ulong first, ulong second);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract bool FirstIsToTheRightOfSecond(ulong first, ulong second);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void IncrementSize(ulong node)
        {
            SetSize(node, GetSize(node) + 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DecrementSize(ulong node)
        {
            SetSize(node, GetSize(node) - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ulong GetSizeOrZero(ulong node)
        {
            return node == 0 ? 0 : GetSize(node);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void FixSize(ulong node)
        {
            SetSize(node, GetSizeOrZero(*GetLeft(node)) + GetSizeOrZero(*GetRight(node)) + 1);
        }

        protected void LeftRotate(ulong* root)
        {
            var right = *GetRight(*root);
            if (right == 0) return;
            SetRight(*root, *GetLeft(right));
            SetLeft(right, *root);
            SetSize(right, GetSize(*root));
            FixSize(*root);
            *root = right;
        }

        protected void RightRotate(ulong* root)
        {
            var left = *GetLeft(*root);
            if (left == 0) return;
            SetLeft(*root, *GetRight(left));
            SetRight(left, *root);
            SetSize(left, GetSize(*root));
            FixSize(*root);
            *root = left;
        }

        public bool Contains(ulong node, ulong root)
        {
            while (root != 0)
            {
                if (FirstIsToTheLeftOfSecond(node, root)) // node.Key < root.Key
                    root = *GetLeft(root);
                else if (FirstIsToTheRightOfSecond(node, root)) // node.Key > root.Key
                    root = *GetRight(root);
                else // node.Key == root.Key
                    return true;
            }
            return false;
        }
    }
}
