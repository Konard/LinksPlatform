using System;
using System.Collections.Generic;

namespace ConsoleTester
{
    public static class TreeStructureExperiments
    {
        class TreeNode
        {
            public TreeNode Left;
            public TreeNode Right;
            public int Size;
            public int Value;

            public override string ToString()
            {
                return string.Format("Value = {0}, Size = {1}", Value, Size);
            }

            public bool Validate()
            {
                if (this == Left)
                {
                    return false;
                }
                if (this == Right)
                {
                    return false;
                }

                int estimatedSize = 1;

                if (Left != null && Left.Size < Size)
                {
                    estimatedSize += Left.Size;
                }
                if (Right != null && Right.Size < Size)
                {
                    estimatedSize += Right.Size;
                }

                if (Size != estimatedSize)
                {
                    return false;
                }
                if (!IsLeftThreaded(this) && Left.Value > Value)
                {
                    return false;
                }
                if (!IsRightThreaded(this) && Right.Value < Value)
                {
                    return false;
                }
                return true;
            }

            public bool TryCatchValue(int value)
            {
                return Value == value
                    || (Left != null && Left.Value == value)
                    || (Right != null && Right.Value == value);
            }
        }

        private static void LeftRotate(ref TreeNode root)
        {
            TreeNode newRoot = root.Right;
            if (newRoot == null) return;
            root.Right = newRoot.Left == root ? newRoot : newRoot.Left;
            newRoot.Left = root;
            newRoot.Size = root.Size;
            root.Size = (IsLeftThreaded(root) ? 0 : root.Left.Size) + (IsRightThreaded(root) ? 0 : root.Right.Size) + 1;
            root = newRoot;
        }

        private static void RightRotate(ref TreeNode root)
        {
            TreeNode newRoot = root.Left;
            if (newRoot == null) return;
            root.Left = newRoot.Right == root ? newRoot : newRoot.Right;
            newRoot.Right = root;
            newRoot.Size = root.Size;
            root.Size = (IsLeftThreaded(root) ? 0 : root.Left.Size) + (IsRightThreaded(root) ? 0 : root.Right.Size) + 1;
            root = newRoot;
        }

        // Добавление элемента "не безопасное", так как функция расчитывает на то, что элемент действительно не существует в дереве
        // Мне такой реализации будет достаточно.
        private static void UnsafeInsert(ref TreeNode treeRoot, int value, TreeNode leftThread = null, TreeNode rightThread = null)
        {
            // Используется для отладки
            if (!treeRoot.Validate())
            {
            }

            if (value > treeRoot.Value)
            {
                if (IsRightThreaded(treeRoot))
                {
                    TreeNode node = new TreeNode()
                    {
                        Left = treeRoot,
                        Right = treeRoot.Right,
                        Size = 1,
                        Value = value
                    };

                    treeRoot.Right = node;
                    treeRoot.Size++; // Допускается увеличивать размер только, если заренее было проверено, что такого элемента не было добавлено

                    return;
                }
                else
                {
                    //TreeNode rightNode = treeRoot.Right;
                    //TreeNode leftNode = treeRoot.Left;

                    //if (value > rightNode.Value)
                    //{
                    //    if (!IsRightThreaded(rightNode) && (IsLeftThreaded(treeRoot) || (rightNode.Right.Size + 1) > leftNode.Size))
                    //        LeftRotate(ref treeRoot);
                    //}
                    //else if (value < rightNode.Value)
                    //{
                    //    if (!IsLeftThreaded(rightNode) && (value > rightNode.Left.Value) && (IsLeftThreaded(treeRoot) || (rightNode.Left.Size + 1) > leftNode.Size))
                    //    {
                    //        RightRotate(ref treeRoot.Right);
                    //        LeftRotate(ref treeRoot);
                    //    }
                    //}

                    leftThread = treeRoot;
                    treeRoot.Size++; // Допускается увеличивать размер только, если заренее было проверено, что такого элемента не было добавлено

                    UnsafeInsert(ref treeRoot.Right, value, leftThread, rightThread);
                }
            }
            else if (value < treeRoot.Value)
            {
                if (IsLeftThreaded(treeRoot))
                {
                    TreeNode node = new TreeNode()
                    {
                        Left = treeRoot.Left,
                        Right = treeRoot,
                        Size = 1,
                        Value = value
                    };

                    treeRoot.Left = node;
                    treeRoot.Size++; // Допускается увеличивать размер только, если заренее было проверено, что такого элемента не было добавлено

                    return;
                }
                else
                {
                    //TreeNode rightNode = treeRoot.Right;
                    //TreeNode leftNode = treeRoot.Left;

                    //if (value < leftNode.Value)
                    //{
                    //    if (!IsLeftThreaded(leftNode) && (IsRightThreaded(treeRoot) || (leftNode.Left.Size + 1) > rightNode.Size))
                    //        RightRotate(ref treeRoot);
                    //}
                    //else if (value > leftNode.Value)
                    //{
                    //    if (!IsRightThreaded(leftNode) && (value < leftNode.Right.Value) && (IsRightThreaded(treeRoot) || (leftNode.Right.Size + 1) > rightNode.Size))
                    //    {
                    //        LeftRotate(ref treeRoot.Left);
                    //        RightRotate(ref treeRoot);
                    //    }
                    //}

                    rightThread = treeRoot;
                    treeRoot.Size++; // Допускается увеличивать размер только, если заренее было проверено, что такого элемента не было добавлено

                    UnsafeInsert(ref treeRoot.Left, value, leftThread, rightThread);
                }
            }
        }

        private static bool IsRightThreaded(TreeNode node)
        {
            return node.Right == null || node.Size <= node.Right.Size;
        }

        private static bool IsLeftThreaded(TreeNode node)
        {
            return node.Left == null || node.Size <= node.Left.Size;
        }

        // Удаление элемента "не безопасное", так как функция расчитывает на то, что элемент действительно существует в дереве
        // Мне такой реализации будет достаточно.
        //private static void UnsafeRemove(ref TreeNode treeRoot, int value)
        //{
        //    if (value > treeRoot.Value)
        //    {
        //        if (!IsRightThreaded(treeRoot))
        //        {
        //            // Переделать оптимизацию
        //            // Оптимизацию нужно развернуть (поменять стороны право и лево) в данный момент это просто копия балансировки из операции вставки

        //            //TreeNode rightNode = treeRoot.Right;
        //            //TreeNode leftNode = treeRoot.Left;

        //            //if (value > rightNode.Value)
        //            //{
        //            //    if (!IsRightThreaded(rightNode) && (IsLeftThreaded(treeRoot) || (rightNode.Right.Size + 1) > leftNode.Size))
        //            //        LeftRotate(ref treeRoot);
        //            //}
        //            //else if (value < rightNode.Value)
        //            //{
        //            //    if (!IsLeftThreaded(rightNode) && (value > rightNode.Left.Value) && (IsLeftThreaded(treeRoot) || (rightNode.Left.Size + 1) > leftNode.Size))
        //            //    {
        //            //        RightRotate(ref treeRoot.Right);
        //            //        LeftRotate(ref treeRoot);
        //            //    }
        //            //}

        //            treeRoot.Size--; // Допускается уменьшать размер только, если заренее было проверено, что такого элемент находится в дереве

        //            UnsafeRemove(ref treeRoot.Right, value);
        //        }
        //    }
        //    else if (value < treeRoot.Value)
        //    {
        //        if (!IsLeftThreaded(treeRoot))
        //        {
        //            // Переделать оптимизацию
        //            // Оптимизацию нужно развернуть (поменять стороны право и лево) в данный момент это просто копия балансировки из операции вставки

        //            //TreeNode rightNode = treeRoot.Right;
        //            //TreeNode leftNode = treeRoot.Left;

        //            //if (value < leftNode.Value)
        //            //{
        //            //    if (!IsLeftThreaded(leftNode) && (IsRightThreaded(treeRoot) || (leftNode.Left.Size + 1) > rightNode.Size))
        //            //        RightRotate(ref treeRoot);
        //            //}
        //            //else if (value > leftNode.Value)
        //            //{
        //            //    if (!IsRightThreaded(leftNode) && (value < leftNode.Right.Value) && (IsRightThreaded(treeRoot) || (leftNode.Right.Size + 1) > rightNode.Size))
        //            //    {
        //            //        LeftRotate(ref treeRoot.Left);
        //            //        RightRotate(ref treeRoot);
        //            //    }
        //            //}

        //            treeRoot.Size--; // Допускается уменьшать размер только, если заренее было проверено, что такого элемент находится в дереве

        //            UnsafeRemove(ref treeRoot.Left, value);
        //        }
        //    }
        //    else // Удаляемый элемент найден
        //    {
        //        TreeNode replacementNode = null;

        //        if (!IsLeftThreaded(treeRoot) && !IsRightThreaded(treeRoot))
        //        {
        //            if (treeRoot.Left.Size > treeRoot.Right.Size)
        //            {
        //                // Нужно отсоединить элемент с минимальным значением, чтобы затем его можно было поставить на место Root-а
        //                TreeNode lastNodeBeforeTreeRoot = treeRoot.Left, lastNodeParent = null;
        //                while (lastNodeBeforeTreeRoot.Right != null && lastNodeBeforeTreeRoot.Size >= lastNodeBeforeTreeRoot.Right.Size)
        //                {
        //                    lastNodeParent = lastNodeBeforeTreeRoot;
        //                    lastNodeParent.Size--;
        //                    lastNodeBeforeTreeRoot = lastNodeBeforeTreeRoot.Right;
        //                }
        //                if (lastNodeParent != null)
        //                {
        //                    lastNodeParent.Right = lastNodeBeforeTreeRoot.Left != lastNodeParent ? lastNodeBeforeTreeRoot.Left : null;

        //                    lastNodeBeforeTreeRoot.Left = treeRoot.Left;
        //                }

        //                lastNodeBeforeTreeRoot.Right = treeRoot.Right;
        //                lastNodeBeforeTreeRoot.Size = treeRoot.Left.Size + treeRoot.Right.Size + 1;

        //                replacementNode = lastNodeBeforeTreeRoot;
        //            }
        //            else if (treeRoot.Left.Size < treeRoot.Right.Size)
        //            {

        //            }
        //            else
        //            {

        //            }
        //        }
        //        else if (!IsLeftThreaded(treeRoot))
        //        {
        //            ... = treeRoot.Right;

        //            replacementNode = treeRoot.Left;
        //        }
        //        else if (!IsRightThreaded(treeRoot))
        //        {
        //            ... = treeRoot.Left;

        //            replacementNode = treeRoot.Right;
        //        }

        //        // Деинициализация не важна в C#, но может быть важна при работе с собственным менеджером памяти
        //        treeRoot.Size = 0;
        //        treeRoot.Left = null;
        //        treeRoot.Right = null;
        //        treeRoot.Value = 0;

        //        treeRoot = replacementNode;
        //    }
        //}

        private static TreeNode Search(TreeNode treeRoot, int value)
        {
            TreeNode currentNode = treeRoot;

            while (true)
            {
                if (value > currentNode.Value)
                    if (IsRightThreaded(currentNode))
                        return null;
                    else
                        currentNode = currentNode.Right;
                else if (value < currentNode.Value)
                    if (IsLeftThreaded(currentNode))
                        return null;
                    else
                        currentNode = currentNode.Left;
                else
                    return currentNode;
            }
        }

        private static TreeNode GetFirstNode(TreeNode treeRoot)
        {
            TreeNode currentNode = treeRoot;
            while (currentNode.Left != null && currentNode.Size >= currentNode.Left.Size) currentNode = currentNode.Left;
            return currentNode;
        }

        private static TreeNode GetLastNode(TreeNode treeRoot)
        {
            TreeNode currentNode = treeRoot;
            while (currentNode.Right != null && currentNode.Size >= currentNode.Right.Size) currentNode = currentNode.Right;
            return currentNode;
        }

        private static TreeNode GetNextNode(TreeNode currentNode)
        {
            if (currentNode.Right != null)
            {
                if (IsRightThreaded(currentNode))
                {
                    return currentNode.Right;
                }
                else
                {
                    currentNode = currentNode.Right;
                    while (currentNode.Left != null && currentNode.Size >= currentNode.Left.Size) currentNode = currentNode.Left;
                    return currentNode;
                }
            }
            else
            {
                return null;
            }
        }

        private static void Travel(TreeNode treeRoot, Action<int> action)
        {
            TreeNode currentNode = GetFirstNode(treeRoot);

            while (currentNode != null)
            {
                action(currentNode.Value);

                currentNode = GetNextNode(currentNode);
            }
        }

        private static IEnumerable<int> GetNodesEnumerator(TreeNode treeRoot)
        {
            TreeNode currentNode = GetFirstNode(treeRoot);

            while (currentNode != null)
            {
                yield return currentNode.Value;

                currentNode = GetNextNode(currentNode);
            }
        }

        public static void RunExperiment()
        {
            int seed = 0;

            //for (; seed < 10000; seed++)
            //{
            Random rnd = new Random(seed);

            // Fill the tree

            TreeNode treeRoot = new TreeNode()
            {
                Size = 1,
                Value = rnd.Next(1000)
            };

            for (int i = 0; i < 128; i++)
            {
                int value = rnd.Next(1000);

                if (Search(treeRoot, value) == null)
                    UnsafeInsert(ref treeRoot, value);
                //else
                //    UnsafeRemove(ref treeRoot, value);
            }

            //Console.WriteLine(seed);


            Travel(treeRoot, (value) =>
            {
                Console.WriteLine(value);
            });

            //}
        }
    }
}
