﻿using System;
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
    public abstract class SizedAndThreadedAVLBalancedTreeMethods<TElement> : BinaryTreeMethodsBase<TElement>
    {
        // TODO: Link with size of TElement
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
            unchecked
            {
                // TODO: Try to use stackalloc as an optimization (requires code generation, because of generics)
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
                            currentNode = GetLeftValue(currentNode);
                        }
                        else
                        {
                            // Threads
                            SetLeft(node, GetLeftValue(currentNode));
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
                            currentNode = GetRightValue(currentNode);
                        }
                        else
                        {
                            // Threads
                            SetRight(node, GetRightValue(currentNode));
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
                    var isLeftNode = !Equals(parent, default(TElement)) && Equals(currentNode, GetLeftValue(parent));

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
        }

        private TElement Balance(TElement node)
        {
            unchecked
            {
                var rootBalance = GetBalance(node);
                if (rootBalance < -1)
                {
                    var left = GetLeftValue(node);
                    if (GetBalance(left) > 0)
                    {
                        SetLeft(node, LeftRotateWithBalance(left));
                        FixSize(node);
                    }
                    node = RightRotateWithBalance(node);
                }
                else if (rootBalance > 1)
                {
                    var right = GetRightValue(node);
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
                var right = GetRightValue(node);

                if (GetLeftIsChild(right))
                    SetRight(node, GetLeftValue(right));
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

                return right;
            }
        }

        protected TElement RightRotateWithBalance(TElement node)
        {
            unchecked
            {
                var left = GetLeftValue(node);

                if (GetRightIsChild(left))
                    SetLeft(node, GetRightValue(left));
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

                return left;
            }
        }

        protected TElement GetNext(TElement node)
        {
            unchecked
            {
                var current = GetRightValue(node);

                if (GetRightIsChild(node))
                    while (GetLeftIsChild(current))
                        current = GetLeftValue(current);

                return current;
            }
        }

        protected TElement GetPrevious(TElement node)
        {
            unchecked
            {
                var current = GetLeftValue(node);

                if (GetLeftIsChild(node))
                    while (GetRightIsChild(current))
                        current = GetRightValue(current);

                return current;
            }
        }

        protected override void DetachCore(IntPtr root, TElement node)
        {
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
                        currentNode = GetLeftValue(currentNode);
                    }
                    else if (FirstIsToTheRightOfSecond(node, currentNode))
                    {
                        if (!GetRightIsChild(currentNode))
                            throw new Exception("Cannot find a node.");

                        DecrementSize(currentNode);
                        path[pathPosition++] = currentNode;
                        currentNode = GetRightValue(currentNode);
                    }
                    else
                        break;
                }

                var parent = path[--pathPosition];
                var balanceNode = parent;

                var isLeftNode = !Equals(parent, default(TElement)) && Equals(currentNode, GetLeftValue(parent));

                if (!GetLeftIsChild(currentNode))
                {
                    if (!GetRightIsChild(currentNode)) // node has no children
                    {
                        if (Equals(parent, default(TElement)))
                            root.SetValue(GetZero());
                        else if (isLeftNode)
                        {
                            SetLeftIsChild(parent, false);
                            SetLeft(parent, GetLeftValue(currentNode));
                            IncrementBalance(parent);
                        }
                        else
                        {
                            SetRightIsChild(parent, false);
                            SetRight(parent, GetRightValue(currentNode));
                            DecrementBalance(parent);
                        }
                    }
                    else // node has a right child
                    {
                        var successor = GetNext(currentNode);
                        SetLeft(successor, GetLeftValue(currentNode));

                        var right = GetRightValue(currentNode);
                        if (Equals(parent, default(TElement)))
                            root.SetValue(right);
                        else if (isLeftNode)
                        {
                            SetLeft(parent, right);
                            IncrementBalance(parent);
                        }
                        else
                        {
                            SetRight(parent, right);
                            DecrementBalance(parent);
                        }
                    }
                }
                else // node has a left child
                {
                    if (!GetRightIsChild(currentNode))
                    {
                        var predecessor = GetPrevious(currentNode);
                        SetRight(predecessor, GetRightValue(currentNode));

                        var leftValue = GetLeftValue(currentNode);
                        if (Equals(parent, default(TElement)))
                            root.SetValue(leftValue);
                        else if (isLeftNode)
                        {
                            SetLeft(parent, leftValue);
                            IncrementBalance(parent);
                        }
                        else
                        {
                            SetRight(parent, leftValue);
                            DecrementBalance(parent);
                        }
                    }
                    else // node has a both children (left and right)
                    {
                        var predecessor = GetLeftValue(currentNode);
                        var successor = GetRightValue(currentNode);

                        var successorParent = currentNode;
                        int previousPathPosition = ++pathPosition;

                        // path[pathPosition] == parent
                        // find the immediately next node (and its parent)
                        while (GetLeftIsChild(successor))
                        {
                            path[++pathPosition] = successorParent = successor;
                            successor = GetLeftValue(successor);
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
                                SetLeft(successorParent, GetRightValue(successor));
                            IncrementBalance(successorParent);

                            SetRightIsChild(successor, true);
                            SetRight(successor, GetRightValue(currentNode));
                        }
                        else
                            DecrementBalance(currentNode);

                        // set the predecessor's successor link to point to the right place
                        while (GetRightIsChild(predecessor))
                            predecessor = GetRightValue(predecessor);
                        SetRight(predecessor, successor);

                        // prepare 'successor' to replace 'node'
                        var left = GetLeftValue(currentNode);
                        SetLeftIsChild(successor, true);
                        SetLeft(successor, left);
                        SetBalance(successor, GetBalance(currentNode));

                        FixSize(successor);

                        if (Equals(parent, default(TElement)))
                            root.SetValue(successor);
                        else if (isLeftNode)
                            SetLeft(parent, successor);
                        else
                            SetRight(parent, successor);
                    }
                }

                // restore balance
                if (!Equals(balanceNode, default(TElement)))
                {
                    while (true)
                    {
                        var balanceParent = path[--pathPosition];
                        isLeftNode = !Equals(balanceParent, default(TElement)) && Equals(balanceNode, GetLeftValue(balanceParent));

                        var currentNodeBalance = GetBalance(balanceNode);

                        if ((currentNodeBalance < -1) || (currentNodeBalance > 1))
                        {
                            balanceNode = Balance(balanceNode);
                            if (Equals(balanceParent, default(TElement)))
                                root.SetValue(balanceNode);
                            else if (isLeftNode)
                                SetLeft(balanceParent, balanceNode);
                            else
                                SetRight(balanceParent, balanceNode);
                        }

                        currentNodeBalance = GetBalance(balanceNode);
                        if ((currentNodeBalance != 0) || Equals(balanceParent, default(TElement)))
                            break;

                        if (isLeftNode)
                            IncrementBalance(balanceParent);
                        else
                            DecrementBalance(balanceParent);

                        balanceNode = balanceParent;
                    }
                }

                ClearNode(node);

                ArrayPool.Free(path);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void ClearNode(TElement node)
        {
            SetLeft(node, GetZero());
            SetRight(node, GetZero());
            SetSize(node, GetZero());
            SetLeftIsChild(node, false);
            SetRightIsChild(node, false);
            SetBalance(node, 0);
        }
    }
}