using System;
using System.Runtime.CompilerServices;
using System.Text;
using Platform.Helpers;

namespace Platform.Data.Core.Collections.Trees
{
    public abstract class BinaryTreeMethodsBase<TElement> : GenericCollectionMethodsBase<TElement>
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
        protected virtual TElement GetLeftOrDefault(TElement node) => GetLeft(node) != IntPtr.Zero ? GetLeft(node).GetValue<TElement>() : default(TElement);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual TElement GetRightOrDefault(TElement node) => GetRight(node) != IntPtr.Zero ? GetRight(node).GetValue<TElement>() : default(TElement);

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

        public void Attach(IntPtr root, TElement node)
        {
            //ValidateSizes(root);

            //Debug.WriteLine("--BeforeAttach--");
            //Debug.WriteLine(PrintNodes(root));
            //Debug.WriteLine("----------------");

            //var sizeBefore = GetSize(root);

            if (ValueEqualToZero(root))
            {
                SetSize(node, GetOne());

                root.SetValue(node);

                return;
            }

            AttachCore(root, node);

            //Debug.WriteLine("--AfterAttach--");
            //Debug.WriteLine(PrintNodes(root));
            //Debug.WriteLine("----------------");

            //ValidateSizes(root);

            //var sizeAfter = GetSize(root);

            //if (!Equals(MathHelpers.Increment(sizeBefore), sizeAfter))
            //    throw new Exception("Tree was broken after attach.");
        }

        protected abstract void AttachCore(IntPtr root, TElement node);

        public void Detach(IntPtr root, TElement node)
        {
            //ValidateSizes(root);

            //Debug.WriteLine("--BeforeDetach--");
            //Debug.WriteLine(PrintNodes(root));
            //Debug.WriteLine("----------------");

            //var sizeBefore = GetSize(root);

            if (ValueEqualToZero(root))
                throw new Exception($"Элемент с {node} не содержится в дереве.");

            DetachCore(root, node);

            //Debug.WriteLine("--AfterDetach--");
            //Debug.WriteLine(PrintNodes(root));
            //Debug.WriteLine("----------------");

            //ValidateSizes(root);

            //var sizeAfter = GetSize(root);

            //if (!Equals(MathHelpers.Decrement(sizeBefore), sizeAfter))
            //    throw new Exception("Tree was broken after detach.");
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
            if (Equals(node, default(TElement)))
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
            if (Equals(node, default(TElement)))
                return;

            var size = GetSize(node);
            var leftSize = GetLeftSize(node);
            var rightSize = GetRightSize(node);

            var expectedSize = MathHelpers.Increment(MathHelpers.Add(leftSize, rightSize));

            if (!Equals(size, expectedSize))
                throw new Exception($"Size of {node} is not valid. Expected size: {expectedSize}, actual size: {size}.");

            ValidateSizes(GetLeftOrDefault(node));
            ValidateSizes(GetRightOrDefault(node));
        }

        public void ValidateSize(TElement node)
        {
            var size = GetSize(node);
            var leftSize = GetLeftSize(node);
            var rightSize = GetRightSize(node);

            var expectedSize = MathHelpers.Increment(MathHelpers.Add(leftSize, rightSize));

            if (!Equals(size, expectedSize))
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
            if (Equals(node, default(TElement)))
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
