#ifndef __LINKS_SIZE_BALANCED_TREE_H__
#define __LINKS_SIZE_BALANCED_TREE_H__

#define DefineTreeLeftRotateMethod(methodName, elementType, GetLeftNode, SetLeftNode, GetRightNode, SetRightNode, GetNodeSize, SetNodeSize)		  \
void methodName(elementType rootIndex)																												  \
{ 																																				  \
	elementType rightNodeIndex = GetRightNode(rootIndex);																								  \
	if (rightNode == NULL) return; 																												  \
	SetRightNode(*root, GetLeftNode(rightNode)); 																								  \
	SetLeftNode(rightNode, *root); 																											  \
	SetNodeSize(rightNode, GetNodeSize(*root));																									  \
	SetNodeSize(*root, (GetLeftNode(*root) ? GetNodeSize(GetLeftNode(*root)) : 0) + (GetRightNode(*root) ? GetNodeSize(GetRightNode(*root)) : 0) + 1);						  \
	*root = rightNode;																												  \
}

#define DefineTreeRightRotateMethod(methodName, elementType, GetLeftNode, SetLeftNode, GetRightNode, SetRightNode, GetNodeSize, SetNodeSize)	  \
void methodName(elementType** root)																												  \
{ 																																				  \
	elementType* leftNode = GetLeftNode(*root);																									  \
	if(leftNode == NULL) return;																													  \
	SetLeftNode(*root, GetRightNode(leftNode));																									  \
	SetRightNode(leftNode, *root);																												  \
	SetNodeSize(leftNode, GetNodeSize(*root));																									  \
	SetNodeSize(*root, (GetLeftNode(*root) ? GetNodeSize(GetLeftNode(*root)) : 0) + (GetRightNode(*root) ? GetNodeSize(GetRightNode(*root)) : 0) + 1);															  \
	*root = leftNode;																															  \
}

#define DefineTreeMaintainMethodsHeaders(LeftMaintain, RightMaintain, elementType)					\
	void LeftMaintain(elementType** root);															\
	void RightMaintain(elementType** root);

#define DefineTreeLeftMaintainMethod(methodName, RightMaintain, elementType, LeftRotate, RightRotate, GetLeftNode, GetRightNode, GetNodeSize)								 \
void methodName(elementType** root)																																			 \
{ 																																											 \
	if (*root) 																																								 \
	{ 																																										 \
		elementType* rootLeftNode = GetLeftNode(*root);																														 \
		if (rootLeftNode) 																																					 \
		{ 																																									 \
			elementType* rootRightNode = GetRightNode(*root);																												 \
			elementType* rootLeftNodeLeftNode = GetLeftNode(rootLeftNode);																									 \
			if(rootLeftNodeLeftNode && (!rootRightNode || GetNodeSize(rootLeftNodeLeftNode) > GetNodeSize(rootRightNode)))													 \
				RightRotate(root); 																																			 \
			else																																							 \
			{																																								 \
				elementType* rootLeftNodeRightNode = GetRightNode(rootLeftNode);																							 \
				if(rootLeftNodeRightNode && (!rootRightNode || GetNodeSize(rootLeftNodeRightNode) > GetNodeSize(rootRightNode))) 											 \
					LeftRotate(&GetLeftNode(*root)), RightRotate(root);																										 \
				else																																						 \
					return; 																																				 \
			}																																								 \
			methodName(&GetLeftNode(*root));																																 \
			RightMaintain(&GetRightNode(*root));																															 \
			methodName(root);																																				 \
			RightMaintain(root);																																			 \
		} 																																									 \
	}																																										 \
}

#define DefineTreeRightMaintainMethod(methodName, LeftMaintain, elementType, LeftRotate, RightRotate, GetLeftNode, GetRightNode, GetNodeSize)								 \
void methodName(elementType** root)																																			 \
{ 																																											 \
	if (*root) 																																								 \
	{ 																																										 \
		elementType* rootRightNode = GetRightNode(*root);																													 \
		if (rootRightNode)																																					 \
		{																																									 \
			elementType* rootLeftNode = GetLeftNode(*root);																													 \
			elementType* rootRightNodeRightNode = GetRightNode(rootRightNode);																								 \
			if (rootRightNodeRightNode && (!rootLeftNode || GetNodeSize(rootRightNodeRightNode) > GetNodeSize(rootLeftNode)))												 \
				LeftRotate(root);																																			 \
			else 																																							 \
			{																																								 \
				elementType* rootRightNodeLeftNode = GetLeftNode(rootRightNode);																							 \
				if (rootRightNodeLeftNode && (!rootLeftNode || GetNodeSize(rootRightNodeLeftNode) > GetNodeSize(rootLeftNode))) 											 \
					RightRotate(&GetRightNode(*root)), LeftRotate(root); 																									 \
				else																																						 \
					return;																																					 \
			}																																								 \
			LeftMaintain(&GetLeftNode(*root));																															 \
			methodName(&GetRightNode(*root));																																 \
			LeftMaintain(root);																																				 \
			methodName(root);																																			 \
		}																																									 \
	}																																										 \
}

#define DefineTreeMaintainMethods(LeftMaintain, RightMaintain, elementType, LeftRotate, RightRotate, GetLeftNode, GetRightNode, GetNodeSize)							\
	void LeftMaintain(elementType** root);																																\
	void RightMaintain(elementType** root);																																\
	DefineTreeLeftMaintainMethod(LeftMaintain, RightMaintain, elementType, LeftRotate, RightRotate, GetLeftNode, GetRightNode, GetNodeSize)								\
	DefineTreeRightMaintainMethod(RightMaintain, LeftMaintain, elementType, LeftRotate, RightRotate, GetLeftNode, GetRightNode, GetNodeSize)							

#define DefineTreeInsertMethod(methodName, elementType, LeftMaintain, RightMaintain, IsElementLessThanOtherElement, GetLeftNode, GetRightNode, GetNodeSize, SetNodeSize)	 \
void methodName(elementType** root, elementType* newNode)																													 \
{ 																																											 \
	if (!*root)																																								 \
	{ 																																										 \
		*root = newNode;																																					 \
		SetNodeSize(*root, GetNodeSize(*root) + 1);																															 \
	}																																										 \
	else 																																									 \
	{ 																																										 \
		SetNodeSize(*root, GetNodeSize(*root) + 1);																															 \
																																											 \
		if(IsElementLessThanOtherElement(newNode, *root))																													 \
		{																																									 \
			methodName(&GetLeftNode(*root), newNode);																														 \
		 	LeftMaintain(root);																																				 \
		}																																									 \
		else																																								 \
		{																																									 \
			methodName(&GetRightNode(*root), newNode);																														 \
			RightMaintain(root);																																			 \
		}																																									 \
	}																																										 \
}

#define DefineUnsafeDetachFromTreeMethod(methodName, elementType, IsElementLessThanOtherElement, IsElementGreaterThanOtherElement, GetLeftNode, SetLeftNode, GetRightNode, SetRightNode, GetNodeSize, SetNodeSize) \
void methodName(elementType** root, elementType* nodeToDetach)																																					   \
{																																																				   \
	if (!*root) 																																																   \
		return;																																																	   \
	else																																																		   \
	{																																																			   \
		elementType* currentNode = *root;																																										   \
		elementType* parent = null; /* Изначально зануление, так как родителя может и не быть (Корень дерева). */																								   \
		elementType* replacementNode = null;																																									   \
																																																				   \
		while (currentNode != nodeToDetach)																																										   \
		{																																																		   \
			SetNodeSize(currentNode, GetNodeSize(currentNode) - 1);																																				   \
			if (IsElementLessThanOtherElement(nodeToDetach, currentNode))																																		   \
				parent = currentNode, currentNode = GetLeftNode(currentNode);																																	   \
			else if (IsElementGreaterThanOtherElement(nodeToDetach, currentNode))																																   \
				parent = currentNode, currentNode = GetRightNode(currentNode);																																	   \
																																																				   \
			/* Проблемная ситуация не обрабатывается специально - её не должно происходить */																													   \
		}																																																		   \
																																																				   \
		if (GetLeftNode(nodeToDetach) && GetRightNode(nodeToDetach))																																			   \
		{																																																		   \
			elementType* minNode = GetRightNode(nodeToDetach);																																					   \
			while (GetLeftNode(minNode)) minNode = GetLeftNode(minNode); /* Передвигаемся до минимума */																										   \
																																																				   \
			methodName(&GetRightNode(nodeToDetach), minNode);																																					   \
																																																				   \
			SetLeftNode(minNode, GetLeftNode(nodeToDetach));																																					   \
			if (GetRightNode(nodeToDetach))																																										   \
			{																																																	   \
				SetRightNode(minNode, GetRightNode(nodeToDetach));																																				   \
				SetNodeSize(minNode, GetNodeSize(GetLeftNode(nodeToDetach)) + GetNodeSize(GetRightNode(nodeToDetach)) + 1);																						   \
			}																																																	   \
			else																																																   \
				SetNodeSize(minNode, GetNodeSize(GetLeftNode(nodeToDetach)) + 1);																																   \
																																																				   \
			replacementNode = minNode;																																											   \
		}																																																		   \
		else if(GetLeftNode(nodeToDetach))																																										   \
			replacementNode = GetLeftNode(nodeToDetach);																																						   \
		else if(GetRightNode(nodeToDetach))																																										   \
			replacementNode = GetRightNode(nodeToDetach);																																						   \
																																																				   \
		if(!parent)																																																   \
			*root = replacementNode;																																											   \
		else if(GetLeftNode(parent) == nodeToDetach)																																							   \
			SetLeftNode(parent, replacementNode);																																								   \
		else if(GetRightNode(parent) == nodeToDetach)																																							   \
			SetRightNode(parent, replacementNode);																																								   \
																																																				   \
		SetNodeSize(nodeToDetach, 0);																																											   \
		SetLeftNode(nodeToDetach, null);																																										   \
		SetRightNode(nodeToDetach, null);																																										   \
	}																																																			   \
}

// Пока что медленнее чем рекурсивная реализация, так как всё что написано на inline асм, компилятор понимает буквально и не пытается
// оптимизировать, до тех пор пока нет точной уверенности в своих знаниях асма, лучше не соваться в дебри.
#define BeginWalkThroughtTree(elementType, element, root, GetLeftNode)			    \
{																					\
	register elementType* element = root;											\
	register initialStackPointer;													\
																					\
	__asm { mov initialStackPointer, ESP }											\
																					\
	while (true)																	\
	{																				\
		if (element != null)														\
		{																			\
			__asm { push element }													\
																					\
			element = GetLeftNode(element);											\
		}																			\
		else																		\
		{																			\
			register int currentStackPointer;										\
			__asm { mov currentStackPointer, ESP }									\
																					\
			if (currentStackPointer == initialStackPointer)							\
				break;																\
			else																	\
			{																		\
				__asm { pop element }													

#define EndWalkThroughtTree(element, GetRightNode)									\
				element = GetRightNode(element);									\
			}																		\
		}																			\
	}																				\
}

#endif
