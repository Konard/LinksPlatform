using System;

namespace Platform.Data.Core.TreeMethods
{
    /// <remarks>
    /// Можно сделать прошитую версию дерева, чтобы сделать проход по дереву более оптимальным.
    /// Также имеет смысл разобраться почему не работает версия с идеальной балансировкой.
    /// </remarks>
    public abstract unsafe class SizeBalancedTreeMethods2 : SizeBalancedTreeMethodsBase
    {
        private void Insert(ulong* root, ulong newNode)
        {
            if (*root == 0)
            {
                *root = newNode;
                SetSize(*root, GetSize(*root) + 1);
            }
            else
            {
                SetSize(*root, GetSize(*root) + 1);

                if (FirstIsToTheLeftOfSecond(newNode, *root))
                {
                    Insert(GetLeft(*root), newNode);
                    LeftMaintain(root);
                }
                else
                {
                    Insert(GetRight(*root), newNode);
                    RightMaintain(root);
                }
            }
        }

        private void Detach(ulong* root, ulong nodeToDetach)
        {
            if (*root == 0)
                return;

            var currentNode = root;
            ulong* parent = null; /* Изначально зануление, так как родителя может и не быть (Корень дерева). */
            ulong replacementNode = 0;

            while (*currentNode != nodeToDetach)
            {
                SetSize(*currentNode, GetSize(*currentNode) - 1);
                if (FirstIsToTheLeftOfSecond(nodeToDetach, *currentNode))
                {
                    parent = currentNode;
                    currentNode = GetLeft(*currentNode);
                }
                else if (FirstIsToTheRightOfSecond(nodeToDetach, *currentNode))
                {
                    parent = currentNode;
                    currentNode = GetRight(*currentNode);
                }

                /* Проблемная ситуация не обрабатывается специально - её не должно происходить */
            }

            if ((*GetLeft(nodeToDetach) != 0) && (*GetRight(nodeToDetach) != 0))
            {
                var minNode = *GetRight(nodeToDetach);
                while (*GetLeft(minNode) != 0) minNode = *GetLeft(minNode); /* Передвигаемся до минимума */

                Detach(GetRight(nodeToDetach), minNode);

                SetLeft(minNode, *GetLeft(nodeToDetach));
                if (*GetRight(nodeToDetach) != 0)
                {
                    SetRight(minNode, *GetRight(nodeToDetach));
                    SetSize(minNode, GetSize(*GetLeft(nodeToDetach)) + GetSize(*GetRight(nodeToDetach)) + 1);
                }
                else
                    SetSize(minNode, GetSize(*GetLeft(nodeToDetach)) + 1);

                replacementNode = minNode;
            }
            else if (*GetLeft(nodeToDetach) != 0)
                replacementNode = *GetLeft(nodeToDetach);
            else if (*GetRight(nodeToDetach) != 0)
                replacementNode = *GetRight(nodeToDetach);

            if (parent == null)
                *root = replacementNode;
            else if (*GetLeft(*parent) == nodeToDetach)
                SetLeft(*parent, replacementNode);
            else if (*GetRight(*parent) == nodeToDetach)
                SetRight(*parent, replacementNode);

            SetSize(nodeToDetach, 0);
            SetLeft(nodeToDetach, 0);
            SetRight(nodeToDetach, 0);
        }

        private void LeftMaintain(ulong* root)
        {
            if (*root != 0)
            {
                var rootLeftNode = GetLeft(*root);
                if (*rootLeftNode != 0)
                {
                    var rootRightNode = GetRight(*root);
                    var rootLeftNodeLeftNode = GetLeft(*rootLeftNode);
                    if ((*rootLeftNodeLeftNode != 0 && *rootLeftNodeLeftNode != 0) &&
                        (*rootRightNode == 0 || GetSize(*rootLeftNodeLeftNode) > GetSize(*rootRightNode)))
                        RightRotate(root);
                    else
                    {
                        var rootLeftNodeRightNode = GetRight(*rootLeftNode);
                        if ((*rootLeftNodeRightNode != 0 && *rootLeftNodeRightNode != 0) &&
                            (*rootRightNode == 0 || GetSize(*rootLeftNodeRightNode) > GetSize(*rootRightNode)))
                        {
                            LeftRotate(GetLeft(*root));
                            RightRotate(root);
                        }
                        else
                            return;
                    }
                    LeftMaintain(GetLeft(*root));
                    RightMaintain(GetRight(*root));
                    LeftMaintain(root);
                    RightMaintain(root);
                }
            }
        }

        private void RightMaintain(ulong* root)
        {
            if (*root != 0)
            {
                var rootRightNode = GetRight(*root);
                if (*rootRightNode != 0)
                {
                    var rootLeftNode = GetLeft(*root);
                    var rootRightNodeRightNode = GetRight(*rootRightNode);
                    if (*rootRightNodeRightNode != 0 &&
                        (*rootLeftNode == 0 || GetSize(*rootRightNodeRightNode) > GetSize(*rootLeftNode)))
                        LeftRotate(root);
                    else
                    {
                        var rootRightNodeLeftNode = GetLeft(*rootRightNode);
                        if (*rootRightNodeLeftNode != 0 &&
                            (*rootLeftNode == 0 || GetSize(*rootRightNodeLeftNode) > GetSize(*rootLeftNode)))
                        {
                            RightRotate(GetRight(*root));
                            LeftRotate(root);
                        }
                        else
                            return;
                    }
                    LeftMaintain(GetLeft(*root));
                    RightMaintain(GetRight(*root));
                    LeftMaintain(root);
                    RightMaintain(root);
                }
            }
        }

        public void AddUnsafe(ulong node, ulong* root)
        {
            if (*root == 0)
            {
                SetSize(node, 1);

                *root = node;

                return;
            }

            Insert(root, node);
        }

        public void RemoveUnsafe(ulong node, ulong* root)
        {
            if (*root == 0)
                throw new Exception(string.Format("Элемент с {0} не содержится в дереве.", node));

            Detach(root, node);
        }
    }
}