using System;

namespace Platform.Links.DataBase.CoreUnsafe.TreeMethods
{
    /// <remarks>
    /// Можно сделать прошитую версию дерева, чтобы сделать проход по дереву более оптимальным.
    /// Также имеет смысл разобраться почему не работает версия с идеальной балансировкой.
    /// </remarks>
    public abstract unsafe class SizeBalancedTreeMethods
    {
        protected abstract ulong* GetLeft(ulong node);
        protected abstract ulong* GetRight(ulong node);
        protected abstract ulong GetSize(ulong node);
        protected abstract void SetLeft(ulong node, ulong left);
        protected abstract void SetRight(ulong node, ulong right);
        protected abstract void SetSize(ulong node, ulong size);
        protected abstract bool FirstIsToTheLeftOfSecond(ulong first, ulong second);
        protected abstract bool FirstIsToTheRightOfSecond(ulong first, ulong second);

        private void IncrementSize(ulong node)
        {
            SetSize(node, GetSize(node) + 1);
        }

        private void DecrementSize(ulong node)
        {
            SetSize(node, GetSize(node) - 1);
        }

        private ulong GetSizeOrZero(ulong node)
        {
            return node == 0 ? 0 : GetSize(node);
        }

        private void FixSize(ulong node)
        {
            SetSize(node, GetSizeOrZero(*GetLeft(node)) + GetSizeOrZero(*GetRight(node)) + 1);
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

        public void AddUnsafe(ulong node, ulong* root)
        {
            if (*root == 0)
            {
                SetSize(node, 1);

                *root = node;

                return;
            }

            while (true)
            {
                var left = GetLeft(*root);
                var leftSize = GetSizeOrZero(*left);
                var right = GetRight(*root);
                var rightSize = GetSizeOrZero(*right);

                if (FirstIsToTheLeftOfSecond(node, *root)) // node.Key < root.Key
                {
                    if (*left == 0)
                    {
                        IncrementSize(*root);

                        SetSize(node, 1);

                        *left = node;

                        break;
                    }
                    if (FirstIsToTheRightOfSecond(node, *left)) // node.Key > left.Key
                    {
                        var leftRight = *GetRight(*left);
                        var leftRightSize = GetSizeOrZero(leftRight);
                        if (leftRightSize + 1 > rightSize)
                        {
                            if (leftRightSize == 0 && rightSize == 0)
                            {
                                SetLeft(node, *left);
                                SetRight(node, *root);
                                SetSize(node, GetSize(*left) + 2); // 2 - размер ветки *root (right) и самого node

                                SetLeft(*root, 0);
                                SetSize(*root, 1);

                                *root = node;

                                break;
                            }
                            LeftRotate(left);
                            RightRotate(root);
                        }
                        else
                        {
                            IncrementSize(*root);

                            root = left;
                        }
                    }
                    else // node.Key < left.Key
                    {
                        var leftLeft = *GetLeft(*left);
                        var leftLeftSize = GetSizeOrZero(leftLeft);
                        if (leftLeftSize + 1 > rightSize)
                        {
                            RightRotate(root);
                        }
                        else
                        {
                            IncrementSize(*root);

                            root = left;
                        }
                    }
                }
                else // node.Key > root.Key
                {
                    if (*right == 0)
                    {
                        IncrementSize(*root);

                        SetSize(node, 1);

                        *right = node;

                        break;
                    }
                    if (FirstIsToTheRightOfSecond(node, *right)) // node.Key > right.Key
                    {
                        var rightRight = *GetRight(*right);
                        var rightRightSize = GetSizeOrZero(rightRight);
                        if (rightRightSize + 1 > leftSize)
                        {
                            LeftRotate(root);
                        }
                        else
                        {
                            IncrementSize(*root);

                            root = right;
                        }
                    }
                    else // node.Key < right.Key
                    {
                        var rightLeft = *GetLeft(*right);
                        var rightLeftSize = GetSizeOrZero(rightLeft);
                        if (rightLeftSize + 1 > leftSize)
                        {
                            if (rightLeftSize == 0 && leftSize == 0)
                            {
                                SetLeft(node, *root);
                                SetRight(node, *right);
                                SetSize(node, GetSize(*right) + 2); // 2 - размер ветки *root (left) и самого node

                                SetRight(*root, 0);
                                SetSize(*root, 1);

                                *root = node;

                                break;
                            }
                            RightRotate(right);
                            LeftRotate(root);
                        }
                        else
                        {
                            IncrementSize(*root);

                            root = right;
                        }
                    }
                }
            }
        }

        public void RemoveUnsafe(ulong node, ulong* root)
        {
            if (*root == 0)
                throw new Exception(string.Format("Элемент с {0} не содержится в дереве.", node));

            RemoveUnsafeCore(node, root);
        }

        private void RemoveUnsafeCore(ulong node, ulong* root)
        {
            while (true)
            {
                var left = GetLeft(*root);
                var leftSize = GetSizeOrZero(*left);
                var right = GetRight(*root);
                var rightSize = GetSizeOrZero(*right);

                if (FirstIsToTheLeftOfSecond(node, *root)) // node.Key < root.Key
                {
                    if (*left == 0)
                        throw new Exception(string.Format("Элемент {0} не содержится в дереве.", node));

                    var rightLeft = *GetLeft(*right);
                    var rightLeftSize = GetSizeOrZero(rightLeft);
                    var rightRight = *GetRight(*right);
                    var rightRightSize = GetSizeOrZero(rightRight);

                    if (rightRightSize > (leftSize - 1))
                    {
                        LeftRotate(root);
                    }
                    else if (rightLeftSize > (leftSize - 1))
                    {
                        RightRotate(right);
                        LeftRotate(root);
                    }
                    else
                    {
                        DecrementSize(*root);

                        root = left;
                    }
                }
                else if (FirstIsToTheRightOfSecond(node, *root)) // node.Key > root.Key
                {
                    if (*right == 0)
                        throw new Exception(string.Format("Элемент {0} не содержится в дереве.", node));

                    var leftLeft = *GetLeft(*left);
                    var leftLeftSize = GetSizeOrZero(leftLeft);
                    var leftRight = *GetRight(*left);
                    var leftRightSize = GetSizeOrZero(leftRight);

                    if (leftLeftSize > (rightSize - 1))
                    {
                        RightRotate(root);
                    }
                    else if (leftRightSize > (rightSize - 1))
                    {
                        LeftRotate(left);
                        RightRotate(root);
                    }
                    else
                    {
                        DecrementSize(*root);

                        root = right;
                    }
                }
                else // key == root.Key;
                {
                    if (leftSize > 0 && rightSize > 0)
                    {
                        if (leftSize > rightSize)
                        {
                            var replacement = *left;
                            while (*GetRight(replacement) != 0)
                                replacement = *GetRight(replacement);

                            RemoveUnsafeCore(replacement, left);

                            SetLeft(replacement, *left);
                            SetRight(replacement, *right);
                            FixSize(replacement);

                            *root = replacement;
                        }
                        else
                        {
                            var replacement = *right;
                            while (*GetLeft(replacement) != 0)
                                replacement = *GetLeft(replacement);

                            RemoveUnsafeCore(replacement, right);

                            SetLeft(replacement, *left);
                            SetRight(replacement, *right);
                            FixSize(replacement);

                            *root = replacement;
                        }
                    }
                    else if (leftSize > 0)
                        *root = *left;
                    else if (rightSize > 0)
                        *root = *right;
                    else
                        *root = 0;

                    SetLeft(node, 0);
                    SetRight(node, 0);
                    SetSize(node, 0);

                    break;
                }
            }
        }

        private void LeftRotate(ulong* root)
        {
            var right = *GetRight(*root);
            if (right == 0) return;
            SetRight(*root, *GetLeft(right));
            SetLeft(right, *root);
            SetSize(right, GetSize(*root));
            FixSize(*root);
            *root = right;
        }

        private void RightRotate(ulong* root)
        {
            var left = *GetLeft(*root);
            if (left == 0) return;
            SetLeft(*root, *GetRight(left));
            SetRight(left, *root);
            SetSize(left, GetSize(*root));
            FixSize(*root);
            *root = left;
        }
    }
}