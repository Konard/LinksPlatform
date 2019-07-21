//#define ENABLE_TREE_AUTO_DEBUG_AND_VALIDATION

using System;
using System.Runtime.CompilerServices;
using System.Text;
using Platform.Numbers;
using Platform.Unsafe;

namespace Platform.Data.Core.Collections.Trees
{
    public abstract class BinaryTreeMethodsBase<TElement> : GenericCollectionMethodsBase<TElement>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract IntPtr GetLeftPointer(TElement node);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract IntPtr GetRightPointer(TElement node);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract TElement GetLeftValue(TElement node);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract TElement GetRightValue(TElement node);
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
        protected virtual TElement GetLeftOrDefault(TElement node) => GetLeftPointer(node) != IntPtr.Zero ? GetLeftValue(node) : default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual TElement GetRightOrDefault(TElement node) => GetRightPointer(node) != IntPtr.Zero ? GetRightValue(node) : default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void IncrementSize(TElement node) => SetSize(node, Increment(GetSize(node)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DecrementSize(TElement node) => SetSize(node, Decrement(GetSize(node)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected TElement GetLeftSize(TElement node) => GetSizeOrZero(GetLeftOrDefault(node));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected TElement GetRightSize(TElement node) => GetSizeOrZero(GetRightOrDefault(node));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected TElement GetSizeOrZero(TElement node) => EqualToZero(node) ? GetZero() : GetSize(node);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void FixSize(TElement node) => SetSize(node, Increment(Add(GetLeftSize(node), GetRightSize(node))));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void LeftRotate(IntPtr root) => root.SetValue(LeftRotate(root.GetValue<TElement>()));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected TElement LeftRotate(TElement root)
        {
            var right = GetRightValue(root);
#if ENABLE_TREE_AUTO_DEBUG_AND_VALIDATION
            if (EqualToZero(right))
                throw new Exception("Right is null.");
#endif
            SetRight(root, GetLeftValue(right));
            SetLeft(right, root);
            SetSize(right, GetSize(root));
            FixSize(root);

            return right;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void RightRotate(IntPtr root) => root.SetValue(RightRotate(root.GetValue<TElement>()));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected TElement RightRotate(TElement root)
        {
            var left = GetLeftValue(root);
#if ENABLE_TREE_AUTO_DEBUG_AND_VALIDATION
            if (EqualToZero(left))
                throw new Exception("Left is null.");
#endif
            SetLeft(root, GetRightValue(left));
            SetRight(left, root);
            SetSize(left, GetSize(root));
            FixSize(root);
            return left;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(TElement node, TElement root)
        {
            while (!EqualToZero(root))
            {
                if (FirstIsToTheLeftOfSecond(node, root)) // node.Key < root.Key
                    root = GetLeftOrDefault(root);
                else if (FirstIsToTheRightOfSecond(node, root)) // node.Key > root.Key
                    root = GetRightOrDefault(root);
                else // node.Key == root.Key
                    return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void ClearNode(TElement node)
        {
            SetLeft(node, GetZero());
            SetRight(node, GetZero());
            SetSize(node, GetZero());
        }

        public void Attach(IntPtr root, TElement node)
        {
#if ENABLE_TREE_AUTO_DEBUG_AND_VALIDATION
            ValidateSizes(root);

            Debug.WriteLine("--BeforeAttach--");
            Debug.WriteLine(PrintNodes(root));
            Debug.WriteLine("----------------");

            var sizeBefore = GetSize(root);
#endif

            if (ValueEqualToZero(root))
            {
                SetSize(node, GetOne());

                root.SetValue(node);

                return;
            }

            AttachCore(root, node);

#if ENABLE_TREE_AUTO_DEBUG_AND_VALIDATION
            Debug.WriteLine("--AfterAttach--");
            Debug.WriteLine(PrintNodes(root));
            Debug.WriteLine("----------------");

            ValidateSizes(root);

            var sizeAfter = GetSize(root);

            if (!IsEquals(MathHelpers.Increment(sizeBefore), sizeAfter))
                throw new Exception("Tree was broken after attach.");
#endif
        }

        protected abstract void AttachCore(IntPtr root, TElement node);

        public void Detach(IntPtr root, TElement node)
        {
#if ENABLE_TREE_AUTO_DEBUG_AND_VALIDATION
            ValidateSizes(root);

            Debug.WriteLine("--BeforeDetach--");
            Debug.WriteLine(PrintNodes(root));
            Debug.WriteLine("----------------");

            var sizeBefore = GetSize(root);

            if (ValueEqualToZero(root))
                throw new Exception($"Элемент с {node} не содержится в дереве.");
#endif

            DetachCore(root, node);

#if ENABLE_TREE_AUTO_DEBUG_AND_VALIDATION
            Debug.WriteLine("--AfterDetach--");
            Debug.WriteLine(PrintNodes(root));
            Debug.WriteLine("----------------");

            ValidateSizes(root);

            var sizeAfter = GetSize(root);

            if (!IsEquals(MathHelpers.Decrement(sizeBefore), sizeAfter))
                throw new Exception("Tree was broken after detach.");
#endif
        }

        protected abstract void DetachCore(IntPtr root, TElement node);

        public TElement GetSize(IntPtr root)
        {
            return root == IntPtr.Zero ? GetZero() : GetSizeOrZero(root.GetValue<TElement>());
        }

        public void FixSizes(IntPtr root)
        {
            if (root != IntPtr.Zero)
                FixSizes(root.GetValue<TElement>());
        }

        public void FixSizes(TElement node)
        {
            if (IsEquals(node, default))
                return;

            FixSizes(GetLeftOrDefault(node));
            FixSizes(GetRightOrDefault(node));

            FixSize(node);
        }

        public void ValidateSizes(IntPtr root)
        {
            if (root != IntPtr.Zero)
                ValidateSizes(root.GetValue<TElement>());
        }

        public void ValidateSizes(TElement node)
        {
            if (IsEquals(node, default))
                return;

            var size = GetSize(node);
            var leftSize = GetLeftSize(node);
            var rightSize = GetRightSize(node);

            var expectedSize = ArithmeticHelpers.Increment(ArithmeticHelpers.Add(leftSize, rightSize));

            if (!IsEquals(size, expectedSize))
                throw new Exception($"Size of {node} is not valid. Expected size: {expectedSize}, actual size: {size}.");

            ValidateSizes(GetLeftOrDefault(node));
            ValidateSizes(GetRightOrDefault(node));
        }

        public void ValidateSize(TElement node)
        {
            var size = GetSize(node);
            var leftSize = GetLeftSize(node);
            var rightSize = GetRightSize(node);

            var expectedSize = ArithmeticHelpers.Increment(ArithmeticHelpers.Add(leftSize, rightSize));

            if (!IsEquals(size, expectedSize))
                throw new Exception($"Size of {node} is not valid. Expected size: {expectedSize}, actual size: {size}.");
        }

        public string PrintNodes(IntPtr root)
        {
            if (root != IntPtr.Zero)
            {
                var sb = new StringBuilder();
                PrintNodes(root.GetValue<TElement>(), sb);
                return sb.ToString();
            }

            return "";
        }

        public string PrintNodes(TElement node)
        {
            var sb = new StringBuilder();
            PrintNodes(node, sb);
            return sb.ToString();
        }

        public void PrintNodes(TElement node, StringBuilder sb, int level = 0)
        {
            if (IsEquals(node, default))
                return;

            PrintNodes(GetLeftOrDefault(node), sb, level + 1);

            PrintNode(node, sb, level);

            sb.AppendLine();

            PrintNodes(GetRightOrDefault(node), sb, level + 1);
        }

        public string PrintNode(TElement node)
        {
            var sb = new StringBuilder();
            PrintNode(node, sb);
            return sb.ToString();
        }

        protected virtual void PrintNode(TElement node, StringBuilder sb, int level = 0)
        {
            sb.Append('\t', level);
            sb.Append(node);
            PrintNodeValue(node, sb);
            sb.Append(' ');
            sb.Append('s');
            sb.Append(GetSize(node));
        }

        protected virtual void PrintNodeValue(TElement node, StringBuilder sb)
        {
        }
    }
}
