using System;
using System.Runtime.CompilerServices;
using System.Text;
using Platform.Data.Core.Exceptions;
using Platform.Helpers;
using Platform.Helpers.Collections;

namespace Platform.Data.Core.Collections.Trees
{
    /// <summary>
    /// Combination of Size, Height (AVL), and threads.
    /// Based on: https://github.com/programmatom/TreeLib/blob/master/TreeLib/TreeLib/Generated/AVLTreeList.cs
    /// Which itself based on: https://github.com/GNOME/glib/blob/master/glib/gtree.c
    /// </summary>
    /// <remarks>
    /// TODO: Compare performance with and without unchecked.
    /// </remarks>
    public abstract class SizeBalancedTreeMethods3<TElement> : SizeBalancedTreeMethodsBase<TElement>
    {
        private const int MaxPath = 92;

        protected override void PrintNode(TElement node, StringBuilder sb, int level = 0)
        {
            base.PrintNode(node, sb, level);

            sb.Append(' ');
            sb.Append(GetLeftIsChild(node) ? 'l' : 'L');
            sb.Append(GetRightIsChild(node) ? 'r' : 'R');

            sb.Append(' ');
            sb.Append(GetBalance(node));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void IncrementBalance(TElement node) => SetBalance(node, (sbyte)(GetBalance(node) + 1));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DecrementBalance(TElement node) => SetBalance(node, (sbyte)(GetBalance(node) - 1));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override TElement GetLeftOrDefault(TElement node) => GetLeftIsChild(node) ? base.GetLeftOrDefault(node) : default(TElement);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override TElement GetRightOrDefault(TElement node) => GetRightIsChild(node) ? base.GetRightOrDefault(node) : default(TElement);

        protected abstract bool GetLeftIsChild(TElement node);
        protected abstract void SetLeftIsChild(TElement node, bool value);
        protected abstract bool GetRightIsChild(TElement node);
        protected abstract void SetRightIsChild(TElement node, bool value);
        protected abstract sbyte GetBalance(TElement node);
        protected abstract void SetBalance(TElement node, sbyte value);

        protected override void AttachCore(IntPtr root, TElement node)
        {
            ValidateSizes(root);

            unchecked
            {
                var path = ArrayPool.Allocate<TElement>(MaxPath);

                var pathPosition = 0;
                path[pathPosition++] = default(TElement);
                var currentNode = root.GetValue<TElement>();

                while (true)
                {
                    if (FirstIsToTheLeftOfSecond(node, currentNode))
                    {
                        if (GetLeftIsChild(currentNode))
                        {
                            IncrementSize(currentNode);
                            path[pathPosition++] = currentNode;
                            currentNode = GetLeft(currentNode).GetValue<TElement>();
                        }
                        else
                        {
                            // Threads
                            SetLeft(node, GetLeft(currentNode).GetValue<TElement>());
                            SetRight(node, currentNode);

                            SetLeft(currentNode, node);
                            SetLeftIsChild(currentNode, true);
                            DecrementBalance(currentNode);

                            SetSize(node, GetOne());
                            FixSize(currentNode); // Should be incremented already
                            break;
                        }
                    }
                    else if (FirstIsToTheRightOfSecond(node, currentNode))
                    {
                        if (GetRightIsChild(currentNode))
                        {
                            IncrementSize(currentNode);
                            path[pathPosition++] = currentNode;
                            currentNode = GetRight(currentNode).GetValue<TElement>();
                        }
                        else
                        {
                            // Threads
                            SetRight(node, GetRight(currentNode).GetValue<TElement>());
                            SetLeft(node, currentNode);

                            SetRight(currentNode, node);
                            SetRightIsChild(currentNode, true);
                            IncrementBalance(currentNode);

                            SetSize(node, GetOne());
                            FixSize(currentNode); // Should be incremented already
                            break;
                        }
                    }
                    else
                        throw new LinkWithSameValueAlreadyExistsException();
                }

                // Restore balance. This is the goodness of a non-recursive
                // implementation, when we are done with balancing we 'break'
                // the loop and we are done.
                while (true)
                {
                    var parent = path[--pathPosition];
                    var isLeftNode = !Equals(parent, default(TElement)) && Equals(currentNode, GetLeft(parent).GetValue<TElement>());

                    var currentNodeBalance = GetBalance(currentNode);

                    if ((currentNodeBalance < -1) || (currentNodeBalance > 1))
                    {
                        currentNode = Balance(currentNode);
                        if (Equals(parent, default(TElement)))
                            root.SetValue(currentNode);
                        else if (isLeftNode)
                        {
                            SetLeft(parent, currentNode);
                            FixSize(parent);
                        }
                        else
                        {
                            SetRight(parent, currentNode);
                            FixSize(parent);
                        }
                    }

                    currentNodeBalance = GetBalance(currentNode);
                    if ((currentNodeBalance == 0) || Equals(parent, default(TElement)))
                        break;

                    if (isLeftNode)
                        DecrementBalance(parent);
                    else
                        IncrementBalance(parent);

                    currentNode = parent;
                }

                ArrayPool.Free(path);
            }

            ValidateSizes(root);
        }

        private TElement Balance(TElement node)
        {
            unchecked
            {
                var rootBalance = GetBalance(node);
                if (rootBalance < -1)
                {
                    var left = GetLeft(node).GetValue<TElement>();
                    if (GetBalance(left) > 0)
                    {
                        SetLeft(node, LeftRotateWithBalance(left));
                        FixSize(node);
                    }
                    node = RightRotateWithBalance(node);
                }
                else if (rootBalance > 1)
                {
                    var right = GetRight(node).GetValue<TElement>();
                    if (GetBalance(right) < 0)
                    {
                        SetRight(node, RightRotateWithBalance(right));
                        FixSize(node);
                    }
                    node = LeftRotateWithBalance(node);
                }

                return node;
            }
        }

        protected TElement LeftRotateWithBalance(TElement node)
        {
            unchecked
            {
                //ValidateSizes(node);

                var right = GetRight(node).GetValue<TElement>();

                if (GetLeftIsChild(right))
                    SetRight(node, GetLeft(right).GetValue<TElement>());
                else
                {
                    SetRightIsChild(node, false);
                    SetLeftIsChild(right, true);
                }

                SetLeft(right, node);

                // Fix size
                SetSize(right, GetSize(node));
                FixSize(node);

                // Fix balance
                var rootBalance = GetBalance(node);
                var rightBalance = GetBalance(right);

                if (rightBalance <= 0)
                {
                    if (rootBalance >= 1)
                        SetBalance(right, (sbyte)(rightBalance - 1));
                    else
                        SetBalance(right, (sbyte)(rootBalance + rightBalance - 2));
                    SetBalance(node, (sbyte)(rootBalance - 1));
                }
                else
                {
                    if (rootBalance <= rightBalance)
                        SetBalance(right, (sbyte)(rootBalance - 2));
                    else
                        SetBalance(right, (sbyte)(rightBalance - 1));
                    SetBalance(node, (sbyte)(rootBalance - rightBalance - 1));
                }

                //ValidateSizes(right);

                return right;
            }
        }

        protected TElement RightRotateWithBalance(TElement node)
        {
            unchecked
            {
                //ValidateSizes(node);

                var left = GetLeft(node).GetValue<TElement>();

                if (GetRightIsChild(left))
                    SetLeft(node, GetRight(left).GetValue<TElement>());
                else
                {
                    SetLeftIsChild(node, false);
                    SetRightIsChild(left, true);
                }

                SetRight(left, node);

                // Fix size
                SetSize(left, GetSize(node));
                FixSize(node);

                // Fix balance
                var rootBalance = GetBalance(node);
                var leftBalance = GetBalance(left);

                if (leftBalance <= 0)
                {
                    if (leftBalance > rootBalance)
                        SetBalance(left, (sbyte)(leftBalance + 1));
                    else
                        SetBalance(left, (sbyte)(rootBalance + 2));
                    SetBalance(node, (sbyte)(rootBalance - leftBalance + 1));
                }
                else
                {
                    if (rootBalance <= -1)
                        SetBalance(left, (sbyte)(leftBalance + 1));
                    else
                        SetBalance(left, (sbyte)(rootBalance + leftBalance + 2));
                    SetBalance(node, (sbyte)(rootBalance + 1));
                }

                //ValidateSizes(left);

                return left;
            }
        }

        private TElement GetNext(TElement node)
        {
            var current = GetRight(node).GetValue<TElement>();

            if (GetRightIsChild(node))
                while (GetLeftIsChild(current))
                    current = GetLeft(current).GetValue<TElement>();

            return current;
        }

        private TElement GetPrevious(TElement node)
        {
            var current = GetLeft(node).GetValue<TElement>();

            if (GetLeftIsChild(node))
                while (GetRightIsChild(current))
                    current = GetRight(current).GetValue<TElement>();

            return current;
        }

        protected override void DetachCore(IntPtr root, TElement node)
        {
            ValidateSizes(root);

            unchecked
            {
                var path = ArrayPool.Allocate<TElement>(MaxPath);
                var pathPosition = 0;
                path[pathPosition++] = default(TElement);

                var currentNode = root.GetValue<TElement>();
                while (true)
                {
                    if (FirstIsToTheLeftOfSecond(node, currentNode))
                    {
                        if (!GetLeftIsChild(currentNode))
                            throw new Exception("Cannot find a node.");

                        DecrementSize(currentNode);
                        path[pathPosition++] = currentNode;
                        currentNode = GetLeft(currentNode).GetValue<TElement>();
                    }
                    else if (FirstIsToTheRightOfSecond(node, currentNode))
                    {
                        if (!GetRightIsChild(currentNode))
                            throw new Exception("Cannot find a node.");

                        DecrementSize(currentNode);
                        path[pathPosition++] = currentNode;
                        currentNode = GetRight(currentNode).GetValue<TElement>();
                    }
                    else
                        break;
                }

                var parent = path[--pathPosition];
                var balanceNode = parent;

                var isLeftNode = !Equals(parent, default(TElement)) && Equals(currentNode, GetLeft(parent).GetValue<TElement>());

                if (!GetLeftIsChild(currentNode))
                {
                    if (!GetRightIsChild(currentNode)) // node has no children
                    {
                        if (Equals(parent, default(TElement)))
                            root.SetValue(GetZero());
                        else if (isLeftNode)
                        {
                            SetLeftIsChild(parent, false);
                            SetLeft(parent, GetLeft(currentNode).GetValue<TElement>());
                            IncrementBalance(parent);
                            //FixSize(parentValue); // Thread Change, No Size Change (should be decremented already)
                        }
                        else
                        {
                            SetRightIsChild(parent, false);
                            SetRight(parent, GetRight(currentNode).GetValue<TElement>());
                            DecrementBalance(parent);
                            //FixSize(parent);  // Thread Change, No Size Change (should be decremented already)
                        }
                    }
                    else // node has a right child
                    {
                        var successor = GetNext(currentNode);
                        SetLeft(successor, GetLeft(currentNode).GetValue<TElement>());
                        //FixSize(successor);  // Thread Change, No Size Change (should be decremented already)

                        var right = GetRight(currentNode).GetValue<TElement>();
                        if (Equals(parent, default(TElement)))
                            root.SetValue(right);
                        else if (isLeftNode)
                        {
                            SetLeft(parent, right);
                            IncrementBalance(parent);
                            //FixSize(parentValue); // Thread Change, No Size Change (should be decremented already)
                        }
                        else
                        {
                            SetRight(parent, right);
                            DecrementBalance(parent);
                            //FixSize(parentValue); // Thread Change, No Size Change (should be decremented already)
                        }
                    }
                }
                else // node has a left child
                {
                    if (!GetRightIsChild(currentNode))
                    {
                        var predecessor = GetPrevious(currentNode);
                        SetRight(predecessor, GetRight(currentNode).GetValue<TElement>());
                        //FixSize(predecessor.GetValue<TElement>()); // Thread Change, No Size Change (should be decremented already)

                        var leftValue = GetLeft(currentNode).GetValue<TElement>();
                        if (Equals(parent, default(TElement)))
                            root.SetValue(leftValue);
                        else if (isLeftNode)
                        {
                            SetLeft(parent, leftValue);
                            IncrementBalance(parent);
                            //FixSize(parentValue); // Thread Change, No Size Change (should be decremented already)
                        }
                        else
                        {
                            SetRight(parent, leftValue);
                            DecrementBalance(parent);
                            //FixSize(parentValue); // Thread Change, No Size Change (should be decremented already)
                        }
                    }
                    else // node has a both children (left and right)
                    {
                        var predecessor = GetLeft(currentNode).GetValue<TElement>();
                        var successor = GetRight(currentNode).GetValue<TElement>();

                        var successorParent = currentNode;
                        int previousPathPosition = ++pathPosition;

                        // path[pathPosition] == parent
                        // find the immediately next node (and its parent)
                        while (GetLeftIsChild(successor))
                        {
                            path[++pathPosition] = successorParent = successor;
                            successor = GetLeft(successor).GetValue<TElement>();
                            if (!Equals(successorParent, currentNode))
                                DecrementSize(successorParent);
                        }

                        path[previousPathPosition] = successor;
                        balanceNode = path[pathPosition];

                        // remove 'successor' from the tree
                        if (!Equals(successorParent, currentNode))
                        {
                            if (!GetRightIsChild(successor))
                                SetLeftIsChild(successorParent, false);
                            else
                            {
                                SetLeft(successorParent, GetRight(successor).GetValue<TElement>());
                                //FixSize(successorParentValue);
                            }
                            IncrementBalance(successorParent);

                            SetRightIsChild(successor, true);
                            SetRight(successor, GetRight(currentNode).GetValue<TElement>());
                            //FixSize(successorValue);
                            //FixSize(successorParentValue); // TODO: Check if this is needed
                        }
                        else
                            DecrementBalance(currentNode);

                        // set the predecessor's successor link to point to the right place
                        while (GetRightIsChild(predecessor))
                            predecessor = GetRight(predecessor).GetValue<TElement>();
                        SetRight(predecessor, successor);

                        // Thread setting should not change size (but this must be checked).
                        //FixSize(predecessor.GetValue<TElement>());

                        // prepare 'successor' to replace 'node'
                        var left = GetLeft(currentNode).GetValue<TElement>();
                        SetLeftIsChild(successor, true);
                        SetLeft(successor, left);
                        SetBalance(successor, GetBalance(currentNode));

                        FixSize(successor);

                        if (Equals(parent, default(TElement)))
                            root.SetValue(successor);
                        else if (isLeftNode)
                        {
                            SetLeft(parent, successor);
                            //FixSize(parentValue); // Should be decremented already
                        }
                        else
                        {
                            SetRight(parent, successor);
                            //FixSize(parentValue); // Should be decremented already
                        }
                    }
                }

                ValidateSizes(root);

                // restore balance
                if (!Equals(balanceNode, default(TElement)))
                {
                    while (true)
                    {
                        var balanceParent = path[--pathPosition];
                        isLeftNode = !Equals(balanceParent, default(TElement)) && Equals(balanceNode, GetLeft(balanceParent).GetValue<TElement>());

                        var currentNodeBalance = GetBalance(balanceNode);

                        if ((currentNodeBalance < -1) || (currentNodeBalance > 1))
                        {
                            balanceNode = Balance(balanceNode);
                            if (Equals(balanceParent, default(TElement)))
                                root.SetValue(balanceNode);
                            else if (isLeftNode)
                            {
                                SetLeft(balanceParent, balanceNode);
                                //FixSize(balanceParent.GetValue<TElement>()); // TODO: Check if this is needed
                            }
                            else
                            {
                                SetRight(balanceParent, balanceNode);
                                //FixSize(balanceParent.GetValue<TElement>()); // TODO: Check if this is needed
                            }
                        }

                        currentNodeBalance = GetBalance(balanceNode);
                        if ((currentNodeBalance != 0) || Equals(balanceParent, default(TElement)))
                            break;

                        if (isLeftNode)
                            IncrementBalance(balanceParent);
                        else
                            DecrementBalance(balanceParent);

                        //FixSize(balanceNodeValue);
                        //if(balanceParent != IntPtr.Zero) FixSize(balanceParent.GetValue<TElement>());

                        balanceNode = balanceParent;
                    }
                }

                // TODO: Make a reset/crear all method
                SetLeft(node, GetZero());
                SetRight(node, GetZero());
                SetSize(node, GetZero());
                SetLeftIsChild(node, false);
                SetRightIsChild(node, false);
                SetBalance(node, 0);

                ArrayPool.Free(path);
            }

            //FixSizes(root); // TODO: Remove hack
            ValidateSizes(root); // TODO: Fix sizes and remove validation
        }
    }
}