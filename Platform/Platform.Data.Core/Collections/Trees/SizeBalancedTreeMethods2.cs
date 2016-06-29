using System;
using Platform.Helpers;

namespace Platform.Data.Core.Collections.Trees
{
    /// <remarks>
    /// Можно сделать прошитую версию дерева, чтобы сделать проход по дереву более оптимальным.
    /// Также имеет смысл разобраться почему не работает версия с идеальной балансировкой.
    /// </remarks>
    public abstract class SizeBalancedTreeMethods2<TElement> : SizeBalancedTreeMethodsBase<TElement>
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
                    AttachCore(GetLeft(root.GetValue<TElement>()), newNode);
                    LeftMaintain(root);
                }
                else
                {
                    AttachCore(GetRight(root.GetValue<TElement>()), newNode);
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

            while (!Equals(currentNode.GetValue<TElement>(), nodeToDetach))
            {
                SetSize(currentNode.GetValue<TElement>(), Decrement(GetSize(currentNode.GetValue<TElement>())));
                if (FirstIsToTheLeftOfSecond(nodeToDetach, currentNode.GetValue<TElement>()))
                {
                    parent = currentNode;
                    currentNode = GetLeft(currentNode.GetValue<TElement>());
                }
                else if (FirstIsToTheRightOfSecond(nodeToDetach, currentNode.GetValue<TElement>()))
                {
                    parent = currentNode;
                    currentNode = GetRight(currentNode.GetValue<TElement>());
                }
                else
                    throw new Exception("Duplicate link found in the tree.");
            }

            if (!ValueEqualToZero(GetLeft(nodeToDetach)) && !ValueEqualToZero(GetRight(nodeToDetach)))
            {
                var minNode = GetRight(nodeToDetach).GetValue<TElement>();
                while (!ValueEqualToZero(GetLeft(minNode)))
                    minNode = GetLeft(minNode).GetValue<TElement>(); /* Передвигаемся до минимума */

                DetachCore(GetRight(nodeToDetach), minNode);

                SetLeft(minNode, GetLeft(nodeToDetach).GetValue<TElement>());
                if (!ValueEqualToZero(GetRight(nodeToDetach)))
                {
                    SetRight(minNode, GetRight(nodeToDetach).GetValue<TElement>());
                    SetSize(minNode, Increment(Add(GetSize(GetLeft(nodeToDetach).GetValue<TElement>()), GetSize(GetRight(nodeToDetach).GetValue<TElement>()))));
                }
                else
                    SetSize(minNode, Increment(GetSize(GetLeft(nodeToDetach).GetValue<TElement>())));

                replacementNode = minNode;
            }
            else if (!ValueEqualToZero(GetLeft(nodeToDetach)))
                replacementNode = GetLeft(nodeToDetach).GetValue<TElement>();
            else if (!ValueEqualToZero(GetRight(nodeToDetach)))
                replacementNode = GetRight(nodeToDetach).GetValue<TElement>();

            if (parent == IntPtr.Zero)
                root.SetValue(replacementNode);
            else if (Equals(GetLeft(parent.GetValue<TElement>()).GetValue<TElement>(), nodeToDetach))
                SetLeft(parent.GetValue<TElement>(), replacementNode);
            else if (Equals(GetRight(parent.GetValue<TElement>()).GetValue<TElement>(), nodeToDetach))
                SetRight(parent.GetValue<TElement>(), replacementNode);

            SetSize(nodeToDetach, GetZero());
            SetLeft(nodeToDetach, GetZero());
            SetRight(nodeToDetach, GetZero());
        }

        private void LeftMaintain(IntPtr root)
        {
            if (!ValueEqualToZero(root))
            {
                var rootLeftNode = GetLeft(root.GetValue<TElement>());
                if (!ValueEqualToZero(rootLeftNode))
                {
                    var rootRightNode = GetRight(root.GetValue<TElement>());
                    var rootLeftNodeLeftNode = GetLeft(rootLeftNode.GetValue<TElement>());
                    if (!ValueEqualToZero(rootLeftNodeLeftNode) &&
                        (ValueEqualToZero(rootRightNode) || GreaterThan(GetSize(rootLeftNodeLeftNode.GetValue<TElement>()), GetSize(rootRightNode.GetValue<TElement>()))))
                        RightRotate(root);
                    else
                    {
                        var rootLeftNodeRightNode = GetRight(rootLeftNode.GetValue<TElement>());
                        if (!ValueEqualToZero(rootLeftNodeRightNode) &&
                            (ValueEqualToZero(rootRightNode) || GreaterThan(GetSize(rootLeftNodeRightNode.GetValue<TElement>()), GetSize(rootRightNode.GetValue<TElement>()))))
                        {
                            LeftRotate(GetLeft(root.GetValue<TElement>()));
                            RightRotate(root);
                        }
                        else
                            return;
                    }
                    LeftMaintain(GetLeft(root.GetValue<TElement>()));
                    RightMaintain(GetRight(root.GetValue<TElement>()));
                    LeftMaintain(root);
                    RightMaintain(root);
                }
            }
        }

        private void RightMaintain(IntPtr root)
        {
            if (!ValueEqualToZero(root))
            {
                var rootRightNode = GetRight(root.GetValue<TElement>());
                if (!ValueEqualToZero(rootRightNode))
                {
                    var rootLeftNode = GetLeft(root.GetValue<TElement>());
                    var rootRightNodeRightNode = GetRight(rootRightNode.GetValue<TElement>());
                    if (!ValueEqualToZero(rootRightNodeRightNode) &&
                        (ValueEqualToZero(rootLeftNode) || GreaterThan(GetSize(rootRightNodeRightNode.GetValue<TElement>()), GetSize(rootLeftNode.GetValue<TElement>()))))
                        LeftRotate(root);
                    else
                    {
                        var rootRightNodeLeftNode = GetLeft(rootRightNode.GetValue<TElement>());
                        if (!ValueEqualToZero(rootRightNodeLeftNode) &&
                            (ValueEqualToZero(rootLeftNode) || GreaterThan(GetSize(rootRightNodeLeftNode.GetValue<TElement>()), GetSize(rootLeftNode.GetValue<TElement>()))))
                        {
                            RightRotate(GetRight(root.GetValue<TElement>()));
                            LeftRotate(root);
                        }
                        else
                            return;
                    }
                    LeftMaintain(GetLeft(root.GetValue<TElement>()));
                    RightMaintain(GetRight(root.GetValue<TElement>()));
                    LeftMaintain(root);
                    RightMaintain(root);
                }
            }
        }
    }
}