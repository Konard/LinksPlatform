#ifndef __LINKS_SIZE_BALANCED_TREE_H__
#define __LINKS_SIZE_BALANCED_TREE_H__

#define DefineTreeLeftRotateMethod(methodName, elementType, GetLeftNode, SetLeftNode, GetRightNode, SetRightNode, GetNodeSize, SetNodeSize)                                     \
void methodName(elementType* rootIndex)                                                                                                                                         \
{                                                                                                                                                                               \
    elementType rightNodeIndex = GetRightNode(*rootIndex);                                                                                                                      \
    if (rightNodeIndex == null) return;                                                                                                                                         \
    SetRightNode(*rootIndex, GetLeftNode(rightNodeIndex));                                                                                                                      \
    SetLeftNode(rightNodeIndex, *rootIndex);                                                                                                                                    \
    SetNodeSize(rightNodeIndex, GetNodeSize(*rootIndex));                                                                                                                       \
    SetNodeSize(*rootIndex, (GetLeftNode(*rootIndex) ? GetNodeSize(GetLeftNode(*rootIndex)) : 0) + (GetRightNode(*rootIndex) ? GetNodeSize(GetRightNode(*rootIndex)) : 0) + 1); \
    *rootIndex = rightNodeIndex;                                                                                                                                                \
}

#define DefineTreeRightRotateMethod(methodName, elementType, GetLeftNode, SetLeftNode, GetRightNode, SetRightNode, GetNodeSize, SetNodeSize)                                    \
void methodName(elementType* rootIndex)                                                                                                                                         \
{                                                                                                                                                                               \
    elementType leftNodeIndex = GetLeftNode(*rootIndex);                                                                                                                        \
    if(leftNodeIndex == null) return;                                                                                                                                           \
    SetLeftNode(*rootIndex, GetRightNode(leftNodeIndex));                                                                                                                       \
    SetRightNode(leftNodeIndex, *rootIndex);                                                                                                                                    \
    SetNodeSize(leftNodeIndex, GetNodeSize(*rootIndex));                                                                                                                        \
    SetNodeSize(*rootIndex, (GetLeftNode(*rootIndex) ? GetNodeSize(GetLeftNode(*rootIndex)) : 0) + (GetRightNode(*rootIndex) ? GetNodeSize(GetRightNode(*rootIndex)) : 0) + 1); \
    *rootIndex = leftNodeIndex;                                                                                                                                                 \
}

#define DefineTreeMaintainMethodsHeaders(LeftMaintain, RightMaintain, elementType) \
    void LeftMaintain(elementType* rootIndex);                                     \
    void RightMaintain(elementType* rootIndex);

#define DefineTreeLeftMaintainMethod(methodName, RightMaintain, elementType, LeftRotate, RightRotate, GetLeftNode, GetRightNode, GetNodeSize)        \
void methodName(elementType* rootIndex)                                                                                                              \
{                                                                                                                                                    \
    if (*rootIndex)                                                                                                                                  \
    {                                                                                                                                                \
        elementType rootLeftNodeIndex = GetLeftNode(*rootIndex);                                                                                     \
        if (rootLeftNodeIndex)                                                                                                                       \
        {                                                                                                                                            \
            elementType rootRightNodeIndex = GetRightNode(*rootIndex);                                                                               \
            elementType rootLeftNodeLeftNodeIndex = GetLeftNode(rootLeftNodeIndex);                                                                  \
            if(rootLeftNodeLeftNodeIndex && (!rootRightNodeIndex || GetNodeSize(rootLeftNodeLeftNodeIndex) > GetNodeSize(rootRightNodeIndex)))       \
                RightRotate(rootIndex);                                                                                                              \
            else                                                                                                                                     \
            {                                                                                                                                        \
                elementType rootLeftNodeRightNodeIndex = GetRightNode(rootLeftNodeIndex);                                                            \
                if(rootLeftNodeRightNodeIndex && (!rootRightNodeIndex || GetNodeSize(rootLeftNodeRightNodeIndex) > GetNodeSize(rootRightNodeIndex))) \
                    LeftRotate(&GetLeftNode(*rootIndex)), RightRotate(rootIndex);                                                                    \
                else                                                                                                                                 \
                    return;                                                                                                                          \
            }                                                                                                                                        \
            methodName(&GetLeftNode(*rootIndex));                                                                                                    \
            RightMaintain(&GetRightNode(*rootIndex));                                                                                                \
            methodName(rootIndex);                                                                                                                   \
            RightMaintain(rootIndex);                                                                                                                \
        }                                                                                                                                            \
    }                                                                                                                                                \
}

#define DefineTreeRightMaintainMethod(methodName, LeftMaintain, elementType, LeftRotate, RightRotate, GetLeftNode, GetRightNode, GetNodeSize)       \
void methodName(elementType* rootIndex)                                                                                                             \
{                                                                                                                                                   \
    if (*rootIndex)                                                                                                                                 \
    {                                                                                                                                               \
        elementType rootRightNodeIndex = GetRightNode(*rootIndex);                                                                                  \
        if (rootRightNodeIndex)                                                                                                                     \
        {                                                                                                                                           \
            elementType rootLeftNodeIndex = GetLeftNode(*rootIndex);                                                                                \
            elementType rootRightNodeRightNodeIndex = GetRightNode(rootRightNodeIndex);                                                             \
            if (rootRightNodeRightNodeIndex && (!rootLeftNodeIndex || GetNodeSize(rootRightNodeRightNodeIndex) > GetNodeSize(rootLeftNodeIndex)))   \
                LeftRotate(rootIndex);                                                                                                              \
            else                                                                                                                                    \
            {                                                                                                                                       \
                elementType rootRightNodeLeftNodeIndex = GetLeftNode(rootRightNodeIndex);                                                           \
                if (rootRightNodeLeftNodeIndex && (!rootLeftNodeIndex || GetNodeSize(rootRightNodeLeftNodeIndex) > GetNodeSize(rootLeftNodeIndex))) \
                    RightRotate(&GetRightNode(*rootIndex)), LeftRotate(rootIndex);                                                                  \
                else                                                                                                                                \
                    return;                                                                                                                         \
            }                                                                                                                                       \
            LeftMaintain(&GetLeftNode(*rootIndex));                                                                                                 \
            methodName(&GetRightNode(*rootIndex));                                                                                                  \
            LeftMaintain(rootIndex);                                                                                                                \
            methodName(rootIndex);                                                                                                                  \
        }                                                                                                                                           \
    }                                                                                                                                               \
}

#define DefineTreeMaintainMethods(LeftMaintain, RightMaintain, elementType, LeftRotate, RightRotate, GetLeftNode, GetRightNode, GetNodeSize) \
    void LeftMaintain(elementType* rootIndex);                                                                                               \
    void RightMaintain(elementType* rootIndex);                                                                                              \
    DefineTreeLeftMaintainMethod(LeftMaintain, RightMaintain, elementType, LeftRotate, RightRotate, GetLeftNode, GetRightNode, GetNodeSize)  \
    DefineTreeRightMaintainMethod(RightMaintain, LeftMaintain, elementType, LeftRotate, RightRotate, GetLeftNode, GetRightNode, GetNodeSize)                            

#define DefineTreeInsertMethod(methodName, elementType, LeftMaintain, RightMaintain, IsElementLessThanOtherElement, GetLeftNode, GetRightNode, GetNodeSize, SetNodeSize) \
void methodName(elementType* rootIndex, elementType newNodeIndex)                                                                                                        \
{                                                                                                                                                                        \
    if (*rootIndex == null)                                                                                                                                              \
    {                                                                                                                                                                    \
        *rootIndex = newNodeIndex;                                                                                                                                       \
        SetNodeSize(*rootIndex, GetNodeSize(*rootIndex) + 1); /* replace with increment */                                                                               \
    }                                                                                                                                                                    \
    else                                                                                                                                                                 \
    {                                                                                                                                                                    \
        SetNodeSize(*rootIndex, GetNodeSize(*rootIndex) + 1); /* replace with increment */                                                                               \
                                                                                                                                                                         \
        if (IsElementLessThanOtherElement(newNodeIndex, *rootIndex))                                                                                                     \
        {                                                                                                                                                                \
            methodName(&GetLeftNode(*rootIndex), newNodeIndex);                                                                                                          \
             LeftMaintain(rootIndex);                                                                                                                                    \
        }                                                                                                                                                                \
        else                                                                                                                                                             \
        {                                                                                                                                                                \
            methodName(&GetRightNode(*rootIndex), newNodeIndex);                                                                                                         \
            RightMaintain(rootIndex);                                                                                                                                    \
        }                                                                                                                                                                \
    }                                                                                                                                                                    \
}

#define DefineUnsafeDetachFromTreeMethod(methodName, elementType, IsElementLessThanOtherElement, IsElementGreaterThanOtherElement, GetLeftNode, SetLeftNode, GetRightNode, SetRightNode, GetNodeSize, SetNodeSize) \
void methodName(elementType* rootIndex, elementType nodeToDetach)                                                                                                                                                  \
{                                                                                                                                                                                                                  \
    if (*rootIndex == null)                                                                                                                                                                                        \
        return;                                                                                                                                                                                                    \
    else                                                                                                                                                                                                           \
    {                                                                                                                                                                                                              \
        elementType currentNode = *rootIndex;                                                                                                                                                                      \
        elementType parent = null; /* Изначально зануление, так как родителя может и не быть (Корень дерева). */                                                                                                   \
        elementType replacementNode = null;                                                                                                                                                                        \
                                                                                                                                                                                                                   \
        while (currentNode != nodeToDetach)                                                                                                                                                                        \
        {                                                                                                                                                                                                          \
            SetNodeSize(currentNode, GetNodeSize(currentNode) - 1);    /* replace with decrement */                                                                                                                \
            if (IsElementLessThanOtherElement(nodeToDetach, currentNode))                                                                                                                                          \
                parent = currentNode, currentNode = GetLeftNode(currentNode);                                                                                                                                      \
            else if (IsElementGreaterThanOtherElement(nodeToDetach, currentNode))                                                                                                                                  \
                parent = currentNode, currentNode = GetRightNode(currentNode);                                                                                                                                     \
                                                                                                                                                                                                                   \
            /* Проблемная ситуация не обрабатывается специально - её не должно происходить */                                                                                                                      \
        }                                                                                                                                                                                                          \
                                                                                                                                                                                                                   \
        if (GetLeftNode(nodeToDetach) && GetRightNode(nodeToDetach))                                                                                                                                               \
        {                                                                                                                                                                                                          \
            elementType minNode = GetRightNode(nodeToDetach);                                                                                                                                                      \
            while (GetLeftNode(minNode)) minNode = GetLeftNode(minNode); /* Передвигаемся до минимума */                                                                                                           \
                                                                                                                                                                                                                   \
            methodName(&GetRightNode(nodeToDetach), minNode);                                                                                                                                                      \
                                                                                                                                                                                                                   \
            SetLeftNode(minNode, GetLeftNode(nodeToDetach));                                                                                                                                                       \
            if (GetRightNode(nodeToDetach))                                                                                                                                                                        \
            {                                                                                                                                                                                                      \
                SetRightNode(minNode, GetRightNode(nodeToDetach));                                                                                                                                                 \
                SetNodeSize(minNode, GetNodeSize(GetLeftNode(nodeToDetach)) + GetNodeSize(GetRightNode(nodeToDetach)) + 1);                                                                                        \
            }                                                                                                                                                                                                      \
            else                                                                                                                                                                                                   \
                SetNodeSize(minNode, GetNodeSize(GetLeftNode(nodeToDetach)) + 1); /* replace with increment */                                                                                                     \
                                                                                                                                                                                                                   \
            replacementNode = minNode;                                                                                                                                                                             \
        }                                                                                                                                                                                                          \
        else if(GetLeftNode(nodeToDetach))                                                                                                                                                                         \
            replacementNode = GetLeftNode(nodeToDetach);                                                                                                                                                           \
        else if(GetRightNode(nodeToDetach))                                                                                                                                                                        \
            replacementNode = GetRightNode(nodeToDetach);                                                                                                                                                          \
                                                                                                                                                                                                                   \
        if(!parent)                                                                                                                                                                                                \
            *rootIndex = replacementNode;                                                                                                                                                                          \
        else if(GetLeftNode(parent) == nodeToDetach)                                                                                                                                                               \
            SetLeftNode(parent, replacementNode);                                                                                                                                                                  \
        else if(GetRightNode(parent) == nodeToDetach)                                                                                                                                                              \
            SetRightNode(parent, replacementNode);                                                                                                                                                                 \
                                                                                                                                                                                                                   \
        SetNodeSize(nodeToDetach, 0);                                                                                                                                                                              \
        SetLeftNode(nodeToDetach, null);                                                                                                                                                                           \
        SetRightNode(nodeToDetach, null);                                                                                                                                                                          \
    }                                                                                                                                                                                                              \
}

// Пока что медленнее чем рекурсивная реализация, так как всё что написано на inline асм, компилятор понимает буквально и не пытается
// оптимизировать, до тех пор пока нет точной уверенности в своих знаниях асма, лучше не соваться в дебри.
#define BeginWalkThroughtTree(elementType, element, root, GetLeftNode) \
{                                                                      \
    register elementType element = root;                               \
    register initialStackPointer;                                      \
                                                                       \
    __asm { mov initialStackPointer, ESP }                             \
                                                                       \
    while (true)                                                       \
    {                                                                  \
        if (element != null)                                           \
        {                                                              \
            __asm { push element }                                     \
                                                                       \
            element = GetLeftNode(element);                            \
        }                                                              \
        else                                                           \
        {                                                              \
            register int currentStackPointer;                          \
            __asm { mov currentStackPointer, ESP }                     \
                                                                       \
            if (currentStackPointer == initialStackPointer)            \
                break;                                                 \
            else                                                       \
            {                                                          \
                __asm { pop element }                                                    

#define EndWalkThroughtTree(element, GetRightNode) \
                element = GetRightNode(element);   \
            }                                      \
        }                                          \
    }                                              \
}

#endif
