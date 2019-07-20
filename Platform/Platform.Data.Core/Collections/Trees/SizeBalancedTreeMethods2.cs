using System;
using Platform.Unsafe;

namespace Platform.Data.Core.Collections.Trees
{
    /// <remarks>
    /// Можно сделать прошитую версию дерева, чтобы сделать проход по дереву более оптимальным.
    /// Также имеет смысл разобраться почему не работает версия с идеальной балансировкой.
    /// </remarks>
    public abstract class SizeBalancedTreeMethods2<TElement> : BinaryTreeMethodsBase<TElement>
    {
        protected override void AttachCore(IntPtr root, TElement newNode)
        {
            if (ValueEqualToZero(root))
            {
                root.SetValue(newNode);
                IncrementSize(root.GetValue<TElement>());
            }
            else
            {
                IncrementSize(root.GetValue<TElement>());

                if (FirstIsToTheLeftOfSecond(newNode, root.GetValue<TElement>()))
                {
                    AttachCore(GetLeftPointer(root.GetValue<TElement>()), newNode);
                    LeftMaintain(root);
                }
                else
                {
                    AttachCore(GetRightPointer(root.GetValue<TElement>()), newNode);
                    RightMaintain(root);
                }
            }
        }

        protected override void DetachCore(IntPtr root, TElement nodeToDetach)
        {
            if (ValueEqualToZero(root))
                return;

            var currentNode = root;
            var parent = IntPtr.Zero; /* Изначально зануление, так как родителя может и не быть (Корень дерева). */
            var replacementNode = GetZero();

            while (!IsEquals(currentNode.GetValue<TElement>(), nodeToDetach))
            {
                SetSize(currentNode.GetValue<TElement>(), Decrement(GetSize(currentNode.GetValue<TElement>())));
                if (FirstIsToTheLeftOfSecond(nodeToDetach, currentNode.GetValue<TElement>()))
                {
                    parent = currentNode;
                    currentNode = GetLeftPointer(currentNode.GetValue<TElement>());
                }
                else if (FirstIsToTheRightOfSecond(nodeToDetach, currentNode.GetValue<TElement>()))
                {
                    parent = currentNode;
                    currentNode = GetRightPointer(currentNode.GetValue<TElement>());
                }
                else
                    throw new Exception("Duplicate link found in the tree.");
            }

            if (!ValueEqualToZero(GetLeftPointer(nodeToDetach)) && !ValueEqualToZero(GetRightPointer(nodeToDetach)))
            {
                var minNode = GetRightValue(nodeToDetach);
                while (!EqualToZero(GetLeftValue(minNode)))
                    minNode = GetLeftValue(minNode); /* Передвигаемся до минимума */

                DetachCore(GetRightPointer(nodeToDetach), minNode);

                SetLeft(minNode, GetLeftValue(nodeToDetach));
                if (!ValueEqualToZero(GetRightPointer(nodeToDetach)))
                {
                    SetRight(minNode, GetRightValue(nodeToDetach));
                    SetSize(minNode, Increment(Add(GetSize(GetLeftValue(nodeToDetach)), GetSize(GetRightValue(nodeToDetach)))));
                }
                else
                    SetSize(minNode, Increment(GetSize(GetLeftValue(nodeToDetach))));

                replacementNode = minNode;
            }
            else if (!ValueEqualToZero(GetLeftPointer(nodeToDetach)))
                replacementNode = GetLeftValue(nodeToDetach);
            else if (!ValueEqualToZero(GetRightPointer(nodeToDetach)))
                replacementNode = GetRightValue(nodeToDetach);

            if (parent == IntPtr.Zero)
                root.SetValue(replacementNode);
            else if (IsEquals(GetLeftValue(parent.GetValue<TElement>()), nodeToDetach))
                SetLeft(parent.GetValue<TElement>(), replacementNode);
            else if (IsEquals(GetRightValue(parent.GetValue<TElement>()), nodeToDetach))
                SetRight(parent.GetValue<TElement>(), replacementNode);

            ClearNode(nodeToDetach);
        }

        private void LeftMaintain(IntPtr root)
        {
            if (!ValueEqualToZero(root))
            {
                var rootLeftNode = GetLeftPointer(root.GetValue<TElement>());
                if (!ValueEqualToZero(rootLeftNode))
                {
                    var rootRightNode = GetRightPointer(root.GetValue<TElement>());
                    var rootLeftNodeLeftNode = GetLeftPointer(rootLeftNode.GetValue<TElement>());
                    if (!ValueEqualToZero(rootLeftNodeLeftNode) &&
                        (ValueEqualToZero(rootRightNode) || GreaterThan(GetSize(rootLeftNodeLeftNode.GetValue<TElement>()), GetSize(rootRightNode.GetValue<TElement>()))))
                        RightRotate(root);
                    else
                    {
                        var rootLeftNodeRightNode = GetRightPointer(rootLeftNode.GetValue<TElement>());
                        if (!ValueEqualToZero(rootLeftNodeRightNode) &&
                            (ValueEqualToZero(rootRightNode) || GreaterThan(GetSize(rootLeftNodeRightNode.GetValue<TElement>()), GetSize(rootRightNode.GetValue<TElement>()))))
                        {
                            LeftRotate(GetLeftPointer(root.GetValue<TElement>()));
                            RightRotate(root);
                        }
                        else
                            return;
                    }
                    LeftMaintain(GetLeftPointer(root.GetValue<TElement>()));
                    RightMaintain(GetRightPointer(root.GetValue<TElement>()));
                    LeftMaintain(root);
                    RightMaintain(root);
                }
            }
        }

        private void RightMaintain(IntPtr root)
        {
            if (!ValueEqualToZero(root))
            {
                var rootRightNode = GetRightPointer(root.GetValue<TElement>());
                if (!ValueEqualToZero(rootRightNode))
                {
                    var rootLeftNode = GetLeftPointer(root.GetValue<TElement>());
                    var rootRightNodeRightNode = GetRightPointer(rootRightNode.GetValue<TElement>());
                    if (!ValueEqualToZero(rootRightNodeRightNode) &&
                        (ValueEqualToZero(rootLeftNode) || GreaterThan(GetSize(rootRightNodeRightNode.GetValue<TElement>()), GetSize(rootLeftNode.GetValue<TElement>()))))
                        LeftRotate(root);
                    else
                    {
                        var rootRightNodeLeftNode = GetLeftPointer(rootRightNode.GetValue<TElement>());
                        if (!ValueEqualToZero(rootRightNodeLeftNode) &&
                            (ValueEqualToZero(rootLeftNode) || GreaterThan(GetSize(rootRightNodeLeftNode.GetValue<TElement>()), GetSize(rootLeftNode.GetValue<TElement>()))))
                        {
                            RightRotate(GetRightPointer(root.GetValue<TElement>()));
                            LeftRotate(root);
                        }
                        else
                            return;
                    }
                    LeftMaintain(GetLeftPointer(root.GetValue<TElement>()));
                    RightMaintain(GetRightPointer(root.GetValue<TElement>()));
                    LeftMaintain(root);
                    RightMaintain(root);
                }
            }
        }
    }
}