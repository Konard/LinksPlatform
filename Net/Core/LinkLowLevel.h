#ifndef __LINKS_LINKLOWLEVEL_H__
#define __LINKS_LINKLOWLEVEL_H__

#define of(x) x
#define to(x) x
#define in(x) x

#define Concat(a, b) a##b
#define Concat3(a, b, c) a##b##c

// Link * -> Index
#define _GetLeftBy(byWhat, linkIndex) GetLink(linkIndex)->Concat(LeftBy,byWhat)
#define _SetLeftBy(byWhat, linkIndex, value) GetLink(linkIndex)->Concat(LeftBy,byWhat) = value
#define _GetRightBy(byWhat, linkIndex) GetLink(linkIndex)->Concat(RightBy,byWhat)
#define _SetRightBy(byWhat, linkIndex, value) GetLink(linkIndex)->Concat(RightBy,byWhat) = value
#define _GetCountBy(byWhat, linkIndex) GetLink(linkIndex)->Concat(CountBy,byWhat)
#define _SetCountBy(byWhat, linkIndex, value) GetLink(linkIndex)->Concat(CountBy,byWhat) = value

// Get

#define __GetLeftBySource(linkIndex) _GetLeftBy(Source, linkIndex)
#define __GetLeftByLinker(linkIndex) _GetLeftBy(Linker, linkIndex)
#define __GetLeftByTarget(linkIndex) _GetLeftBy(Target, linkIndex)

#define __GetRightBySource(linkIndex) _GetRightBy(Source, linkIndex)
#define __GetRightByLinker(linkIndex) _GetRightBy(Linker, linkIndex)
#define __GetRightByTarget(linkIndex) _GetRightBy(Target, linkIndex)

#define __GetCountBySource(linkIndex) _GetCountBy(Source, linkIndex)
#define __GetCountByLinker(linkIndex) _GetCountBy(Linker, linkIndex)
#define __GetCountByTarget(linkIndex) _GetCountBy(Target, linkIndex)

// Set

#define __SetLeftBySource(linkIndex, value) _SetLeftBy(Source, linkIndex, value)
#define __SetLeftByLinker(linkIndex, value) _SetLeftBy(Linker, linkIndex, value)
#define __SetLeftByTarget(linkIndex, value) _SetLeftBy(Target, linkIndex, value)

#define __SetRightBySource(linkIndex, value) _SetRightBy(Source, linkIndex, value)
#define __SetRightByLinker(linkIndex, value) _SetRightBy(Linker, linkIndex, value)
#define __SetRightByTarget(linkIndex, value) _SetRightBy(Target, linkIndex, value)

#define __SetCountBySource(linkIndex, value) _SetCountBy(Source, linkIndex, value)
#define __SetCountByLinker(linkIndex, value) _SetCountBy(Linker, linkIndex, value)
#define __SetCountByTarget(linkIndex, value) _SetCountBy(Target, linkIndex, value)


// Заменил Next на Right
// Аргумент byWhat == SourceIndex, например
// newValue - это уже индекс, а не ссылка
#define _GetNextSiblingRefererBy(byWhat, linkIndex) GetLink(linkIndex)->Concat3(By,byWhat,RightIndex)
#define _SetNextSiblingRefererBy(byWhat, linkIndex, newValue) GetLink(linkIndex)->Concat3(By,byWhat,RightIndex) = newValue

// Заменил Prev на Left
#define _GetPreviousSiblingRefererBy(byWhat, linkIndex) GetLink(linkIndex)->Concat3(By,byWhat,LeftIndex)
#define _SetPreviousSiblingRefererBy(byWhat, linkIndex, newValue) GetLink(linkIndex)->Concat3(By,byWhat,LeftIndex) = newValue

// ByLinker, например
#define _GetFirstRefererBy(byWhat, linkIndex) GetLink(linkIndex)->Concat3(By,byWhat,RootIndex)
#define _SetFirstRefererBy(byWhat, linkIndex, newValue) GetLink(linkIndex)->Concat3(By,byWhat,RootIndex) = newValue

// BySourceCount, например
#define _IncrementNumberOfReferers(whichRererersBy, linkIndex) GetLink(linkIndex)->Concat3(By,whichRererersBy,Count)++
#define _DecrementNumberOfReferers(whichRererersBy, linkIndex) GetLink(linkIndex)->Concat3(By,whichRererersBy,Count)--

// ByLinkerCount, например
#define _GetNumberOfReferersBy(that, linkIndex) GetLink(linkIndex)->Concat(CountBy,that)
#define _SetNumberOfReferersBy(that, linkIndex, newValue) GetLink(linkIndex)->Concat(CountBy,that) = newValue

// заменены link на linkIndex, Source на SourceIndex и т.п.
#define __GetNextSiblingRefererBySource(linkIndex) _GetNextSiblingRefererBy(Source, linkIndex)
#define __GetNextSiblingRefererByLinker(linkIndex) _GetNextSiblingRefererBy(Linker, linkIndex)
#define __GetNextSiblingRefererByTarget(linkIndex) _GetNextSiblingRefererBy(Target, linkIndex)

// заменены Source на SourceIndex и т.п.
#define __SetNextSiblingRefererBySource(linkIndex, newValue) _SetNextSiblingRefererBy(Source, linkIndex, newValue)
#define __SetNextSiblingRefererByLinker(linkIndex, newValue) _SetNextSiblingRefererBy(Linker, linkIndex, newValue)
#define __SetNextSiblingRefererByTarget(linkIndex, newValue) _SetNextSiblingRefererBy(Target, linkIndex, newValue)

// заменены Source на SourceIndex и т.п.
#define __GetPreviousSiblingRefererBySource(linkIndex) _GetPreviousSiblingRefererBy(Source, linkIndex)
#define __GetPreviousSiblingRefererByLinker(linkIndex) _GetPreviousSiblingRefererBy(Linker, linkIndex)
#define __GetPreviousSiblingRefererByTarget(linkIndex) _GetPreviousSiblingRefererBy(Target, linkIndex)

// заменены Source на SourceIndex и т.п.
#define __SetPreviousSiblingRefererBySource(linkIndex, newValue) _SetPreviousSiblingRefererBy(Source, linkIndex, newValue)
#define __SetPreviousSiblingRefererByLinker(linkIndex, newValue) _SetPreviousSiblingRefererBy(Linker, linkIndex, newValue)
#define __SetPreviousSiblingRefererByTarget(linkIndex, newValue) _SetPreviousSiblingRefererBy(Target, linkIndex, newValue)

// +Index
#define __GetNumberOfReferersBySource(linkIndex) _GetNumberOfReferersBy(Source, linkIndex)
#define __GetNumberOfReferersByLinker(linkIndex) _GetNumberOfReferersBy(Linker, linkIndex)
#define __GetNumberOfReferersByTarget(linkIndex) _GetNumberOfReferersBy(Target, linkIndex)

// +Index
#define __SetNumberOfReferersBySource(linkIndex, newValue) _SetNumberOfReferersBy(Source, linkIndex, newValue)
#define __SetNumberOfReferersByLinker(linkIndex, newValue) _SetNumberOfReferersBy(Linker, linkIndex, newValue)
#define __SetNumberOfReferersByTarget(linkIndex, newValue) _SetNumberOfReferersBy(Target, linkIndex, newValue)


// +Index
#define BeginWalkThroughReferersBySource(elementIndex, linkIndex) BeginWalkThroughLinksList(elementIndex, _GetFirstRefererBy(Source, linkIndex))
#define EndWalkThroughReferersBySource(elementIndex) EndWalkThroughLinksList(elementIndex, __GetNextSiblingRefererBySource)

#define BeginWalkThroughReferersByLinker(elementIndex, linkIndex) BeginWalkThroughLinksList(elementIndex, _GetFirstRefererBy(Linker, linkIndex))
#define EndWalkThroughReferersByLinker(elementIndex) EndWalkThroughLinksList(elementIndex, __GetNextSiblingRefererByLinker)

#define BeginWalkThroughReferersByTarget(elementIndex, linkIndex) BeginWalkThroughLinksList(elementIndex, _GetFirstRefererBy(Target, linkIndex))
#define EndWalkThroughReferersByTarget(elementIndex) EndWalkThroughLinksList(elementIndex, __GetNextSiblingRefererBySource)

// +Index
#define BeginWalkThroughLinksList(elementIndex, firstIndex)																		\
{																														\
	link_index firstElementIndex = firstIndex;																							\
	if (firstElementIndex != null) 																							\
	{																													\
		link_index elementIndex = firstElementIndex;																					\
		do																												\
		{																												

// +Index
#define EndWalkThroughLinksList(elementIndex, nextSelector)																	\
			elementIndex = nextSelector(elementIndex);																			\
		}																												\
		while (elementIndex != firstElementIndex);																				\
	}																													\
}

// +Index
#define UnSubscribeFromListOfReferersBy(that, linkIndex, previousValue) 																		\
{																														\
	link_index nextRefererIndex = _GetNextSiblingRefererBy(that,linkIndex);																\
																														\
	if (nextRefererIndex != linkIndex)																							\
	{																													\
		link_index previousRefererIndex = _GetPreviousSiblingRefererBy(that,linkIndex);													\
																														\
		_SetPreviousSiblingRefererBy(that, of(nextRefererIndex), to(previousRefererIndex));										\
		_SetNextSiblingRefererBy(that, of(previousRefererIndex), to(nextRefererIndex));											\
																														\
		if (_GetFirstRefererBy(that, of(previousValue)) == linkIndex)															\
			_SetFirstRefererBy(that, of(previousValue), to(nextRefererIndex));												\
	}																													\
	else if (_GetFirstRefererBy(that, of(previousValue)) == linkIndex)														\
		_SetFirstRefererBy(that, of(previousValue), to(null)); 															\
																														\
	_DecrementNumberOfReferers(that, of(previousValue));																	 \
	_SetNextSiblingRefererBy(that, of(linkIndex), to(null));																		  \
	_SetPreviousSiblingRefererBy(that, of(linkIndex), to(null));																	  \
}																														

// +Index
#define SubscribeToListOfReferersBy(that, linkIndex, newValue)																\
{																														\
	link_index previousFirstRefererIndex = _GetFirstRefererBy(that, of(newValue));												\
																														\
	if (previousFirstRefererIndex != null)																					\
	{																													\
		link_index previousLastRefererIndex = _GetPreviousSiblingRefererBy(that, of(previousFirstRefererIndex));						\
																														\
		_SetNextSiblingRefererBy(that, of(linkIndex), to(previousFirstRefererIndex));												\
		_SetPreviousSiblingRefererBy(that, of(previousFirstRefererIndex), to(linkIndex));											\
																														\
		_SetPreviousSiblingRefererBy(that, of(linkIndex), to(previousLastRefererIndex));											\
		_SetNextSiblingRefererBy(that, of(previousLastRefererIndex), to(linkIndex));												\
	}																													\
	else																												\
	{																													\
		_SetNextSiblingRefererBy(that, of(linkIndex), to(linkIndex));																\
		_SetPreviousSiblingRefererBy(that, of(linkIndex), to(linkIndex));															\
	}																													\
																														\
	_SetFirstRefererBy(that, of(newValue), to(linkIndex));																	\
																														\
	_IncrementNumberOfReferers(that, of(newValue));																		\
}

// +Index
#define IsRefererLessThanOtherRefererBySourceCore(refererIndex, otherRefererLinkerIndex, otherRefererTargetIndex) (GetLinkerIndex(refererIndex) < (otherRefererLinkerIndex) || (GetLinkerIndex(refererIndex) == (otherRefererLinkerIndex) && GetTargetIndex(refererIndex) < (otherRefererTargetIndex)))
#define IsRefererGreaterThanOtherRefererBySourceCore(refererIndex, otherRefererLinkerIndex, otherRefererTargetIndex) (GetLinkerIndex(refererIndex) > (otherRefererLinkerIndex) || (GetLinkerIndex(refererIndex) == (otherRefererLinkerIndex) && GetTargetIndex(refererIndex) > (otherRefererTargetIndex)))
// +Index
#define IsRefererLessThanOtherRefererBySource(refererIndex, otherRefererIndex) IsRefererLessThanOtherRefererBySourceCore(refererIndex, GetLinkerIndex(otherRefererIndex), GetTargetIndex(otherRefererIndex))
#define IsRefererGreaterThanOtherRefererBySource(refererIndex, otherRefererIndex) IsRefererGreaterThanOtherRefererBySourceCore(refererIndex, GetLinkerIndex(otherRefererIndex), GetTargetIndex(otherRefererIndex))

// +Index
#define IsRefererLessThanOtherRefererByLinkerCore(refererIndex, otherRefererSourceIndex, otherRefererTargetIndex) (GetSourceIndex(refererIndex) < (otherRefererSourceIndex) || (GetSourceIndex(refererIndex) == (otherRefererSourceIndex) && GetTargetIndex(refererIndex) < (otherRefererTargetIndex)))
#define IsRefererGreaterThanOtherRefererByLinkerCore(refererIndex, otherRefererSourceIndex, otherRefererTargetIndex) (GetSourceIndex(refererIndex) > (otherRefererSourceIndex) || (GetSourceIndex(refererIndex) == (otherRefererSourceIndex) && GetTargetIndex(refererIndex) > (otherRefererTargetIndex)))
// +Index
#define IsRefererLessThanOtherRefererByLinker(refererIndex, otherRefererIndex) IsRefererLessThanOtherRefererByLinkerCore(refererIndex, GetSourceIndex(otherRefererIndex), GetTargetIndex(otherRefererIndex))
#define IsRefererGreaterThanOtherRefererByLinker(refererIndex, otherRefererIndex) IsRefererGreaterThanOtherRefererByLinkerCore(refererIndex, GetSourceIndex(otherRefererIndex), GetTargetIndex(otherRefererIndex))

// +Index
#define IsRefererLessThanOtherRefererByTargetCore(refererIndex, otherRefererLinkerIndex, otherRefererSourceIndex) (GetLinkerIndex(refererIndex) < (otherRefererLinkerIndex) || (GetLinkerIndex(refererIndex) == (otherRefererLinkerIndex) && GetSourceIndex(refererIndex) < (otherRefererSourceIndex)))
#define IsRefererGreaterThanOtherRefererByTargetCore(refererIndex, otherRefererLinkerIndex, otherRefererSourceIndex) (GetLinkerIndex(refererIndex) > (otherRefererLinkerIndex) || (GetLinkerIndex(refererIndex) == (otherRefererLinkerIndex) && GetSourceIndex(refererIndex) > (otherRefererSourceIndex)))
// +Index
#define IsRefererLessThanOtherRefererByTarget(refererIndex, otherRefererIndex) IsRefererLessThanOtherRefererByTargetCore(refererIndex, GetLinkerIndex(otherRefererIndex), GetSourceIndex(otherRefererIndex))
#define IsRefererGreaterThanOtherRefererByTarget(refererIndex, otherRefererIndex) IsRefererGreaterThanOtherRefererByTargetCore(refererIndex, GetLinkerIndex(otherRefererIndex), GetSourceIndex(otherRefererIndex))

// method(Source,Linker,Target)
#define DefineReferersTreeLeftRotateMethod(referesToThat) \
	DefineTreeLeftRotateMethod( \
		Concat3(By,referesToThat,TreeLeftRotate), \
		link_index, \
		Concat(__GetLeftBy,referesToThat),Concat(__SetLeftBy,referesToThat), \
		Concat(__GetRightBy,referesToThat),Concat(__SetRightBy,referesToThat), \
		Concat(__GetCountBy,referesToThat),Concat(__SetCountBy,referesToThat))

// OK, Concat3(ReferersBy -> Concat3(By
#define DefineReferersTreeRightRotateMethod(referesToThat) \
	DefineTreeRightRotateMethod( \
		Concat3(By, referesToThat, TreeRightRotate), \
		link_index, \
		Concat(__GetLeftBy, referesToThat), Concat(__SetLeftBy, referesToThat), \
		Concat(__GetRightBy, referesToThat), Concat(__SetRightBy, referesToThat), \
		Concat(__GetCountBy, referesToThat), Concat(__SetCountBy, referesToThat))

// OK, Concat3(ReferersBy -> Concat3(By
#define DefineReferersTreeMaintainMethod(referesToThat) \
	DefineTreeMaintainMethods( \
		Concat3(By, referesToThat, TreeLeftMaintain), \
		Concat3(By, referesToThat, TreeRightMaintain), \
		link_index, \
		Concat3(By, referesToThat, TreeLeftRotate), \
		Concat3(By, referesToThat, TreeRightRotate), \
		Concat(__GetPreviousSiblingRefererBy, referesToThat), \
		Concat(__GetNextSiblingRefererBy, referesToThat), \
		Concat(__GetNumberOfReferersBy, referesToThat))

// OK, Concat3(ReferersBy -> Concat3(By
#define DefineReferersTreeInsertMethod(referesToThat) \
	DefineTreeInsertMethod(Concat3(By, referesToThat, TreeInsert), link_index, \
		Concat3(By, referesToThat, TreeLeftMaintain), \
		Concat3(By, referesToThat, TreeRightMaintain), \
		Concat(IsRefererLessThanOtherRefererBy, referesToThat), \
		Concat(__GetPreviousSiblingRefererBy, referesToThat), \
		Concat(__GetNextSiblingRefererBy, referesToThat), \
		Concat(__GetNumberOfReferersBy, referesToThat), \
		Concat(__SetNumberOfReferersBy, referesToThat))

// OK
#define DefineUnsafeDetachFromReferersTreeMethod(referesToThat) \
	DefineUnsafeDetachFromTreeMethod(Concat(UnsafeDetachFromTreeOfReferersBy, referesToThat), link_index, \
		Concat(IsRefererLessThanOtherRefererBy, referesToThat), \
		Concat(IsRefererGreaterThanOtherRefererBy, referesToThat), \
		Concat(__GetPreviousSiblingRefererBy, referesToThat), Concat(__SetPreviousSiblingRefererBy, referesToThat), \
		Concat(__GetNextSiblingRefererBy, referesToThat), Concat(__SetNextSiblingRefererBy, referesToThat), \
		Concat(__GetNumberOfReferersBy, referesToThat), Concat(__SetNumberOfReferersBy, referesToThat)) 

#define DefineAllReferersTreeMethods(referesToThat) \
	DefineReferersTreeLeftRotateMethod(referesToThat) \
	DefineReferersTreeRightRotateMethod(referesToThat) \
	DefineReferersTreeMaintainMethod(referesToThat) \
	DefineReferersTreeInsertMethod(referesToThat) \
	DefineUnsafeDetachFromReferersTreeMethod(referesToThat) \

// OK
#define DefineAllReferersBySourceTreeMethods() DefineAllReferersTreeMethods(Source)
#define DefineAllReferersByLinkerTreeMethods() DefineAllReferersTreeMethods(Linker)
#define DefineAllReferersByTargetTreeMethods() DefineAllReferersTreeMethods(Target)

#define BeginWalkThroughtReferersTree(byThat, element, root) BeginWalkThroughtTree(link_index, element, _GetFirstRefererBy(byThat, root), Concat(__GetPreviousSiblingRefererBy, byThat))
#define EndWalkThroughtReferersTree(byThat, element) EndWalkThroughtTree(element, Concat(__GetNextSiblingRefererBy, byThat))

#define BeginWalkThroughtTreeOfReferersBySource(element, root) BeginWalkThroughtReferersTree(Source, element, root)
#define EndWalkThroughtTreeOfReferersBySource(element) EndWalkThroughtReferersTree(Source, element)

#define BeginWalkThroughtTreeOfReferersByLinker(element, root) BeginWalkThroughtReferersTree(Linker, element, root)
#define EndWalkThroughtTreeOfReferersByLinker(element) EndWalkThroughtReferersTree(Linker, element)

#define BeginWalkThroughtTreeOfReferersByTarget(element, root) BeginWalkThroughtReferersTree(Target, element, root)
#define EndWalkThroughtTreeOfReferersByTarget(element) EndWalkThroughtReferersTree(Target, element)

#define SubscribeToTreeOfReferersBy(that, linkIndex, newValue) Concat3(By, that, TreeInsert)(&_GetFirstRefererBy(that, of(newValue)), linkIndex)
#define UnSubscribeFromTreeOfReferersBy(that, linkIndex, newValue) Concat(UnsafeDetachFromTreeOfReferersBy, that)(&_GetFirstRefererBy(that, of(newValue)), linkIndex)

#define SubscribeAsRefererToSource(linkIndex, newValue) SubscribeToTreeOfReferersBy(Source, linkIndex, newValue)
#define SubscribeAsRefererToLinker(linkIndex, newValue) SubscribeToListOfReferersBy(Linker, linkIndex, newValue)
#define SubscribeAsRefererToTarget(linkIndex, newValue) SubscribeToTreeOfReferersBy(Target, linkIndex, newValue)

#define UnSubscribeFromSource(linkIndex, newValue) UnSubscribeFromTreeOfReferersBy(Source, linkIndex, newValue)
#define UnSubscribeFromLinker(linkIndex, newValue) UnSubscribeFromListOfReferersBy(Linker, linkIndex, newValue)
#define UnSubscribeFromTarget(linkIndex, newValue) UnSubscribeFromTreeOfReferersBy(Target, linkIndex, newValue)

// ReferersBy -> By
////
#define GetNumberOfReferersInList(that, linkIndex) GetLink(linkIndex)->Concat(CountBy,that)
#define GetNumberOfReferersInTree(that, linkIndex) _GetFirstRefererBy(that, of(linkIndex)) ? _GetNumberOfReferersBy(that, _GetFirstRefererBy(that, of(linkIndex))) : 0

#define GetNumberOfReferersBySource(linkIndex) GetNumberOfReferersInTree(Source, linkIndex)
#define GetNumberOfReferersByLinker(linkIndex) GetNumberOfReferersInList(Linker, linkIndex)
#define GetNumberOfReferersByTarget(linkIndex) GetNumberOfReferersInTree(Target, linkIndex)

// Link * -> link_index
#define DefineSearchInListOfReferersBySourceMethod()															 \
link_index SearchRefererOfSource(link_index linkIndex, link_index refererTargetIndex, link_index refererLinkerIndex)								 \
{																												 \
	BeginWalkThroughReferersByTarget(referer, in(linkIndex))															 \
		if (GetTargetIndex(referer) == refererTargetIndex && GetLinkerIndex(referer) == refererLinkerIndex)								 \
			return referer;																						 \
	EndWalkThroughReferersByTarget(referer);																	 \
	return null;																								 \
}

// Link * -> link_index
#define DefineSearchInListOfReferersByLinkerMethod()															 \
link_index SearchRefererOfLinker(link_index linkIndex, link_index refererSourceIndex, link_index refererTargetIndex)								 \
{																												 \
	BeginWalkThroughReferersByTarget(referer, in(linkIndex))															 \
		if (GetSourceIndex(referer) == refererSourceIndex && GetTargetIndex(referer) == refererTargetIndex)								 \
			return referer;																						 \
	EndWalkThroughReferersByTarget(referer);																	 \
	return null;																								 \
}

// Link * -> link_index
#define DefineSearchInListOfReferersByTargetMethod()															 \
link_index SearchRefererOfTarget(link_index linkIndex, link_index refererSourceIndex, link_index refererLinkerIndex)								 \
{																												 \
	BeginWalkThroughReferersByTarget(referer, in(linkIndex))															 \
		if (GetSourceIndex(referer) == refererSourceIndex && GetLinkerIndex(referer) == refererLinkerIndex)								 \
			return referer;																						 \
	EndWalkThroughReferersByTarget(referer);																	 \
	return null;																								 \
}

#define DefineSearchInTreeOfReferersBySourceMethod()															 \
link_index SearchRefererOfSource(link_index linkIndex, link_index refererTargetIndex, link_index refererLinkerIndex)								 \
{																												 \
	link_index currentNode = _GetFirstRefererBy(Source, of(linkIndex));													 \
																												 \
	while (currentNode)																							 \
		if (IsRefererGreaterThanOtherRefererBySourceCore(currentNode, refererLinkerIndex, refererTargetIndex))			 \
			currentNode = _GetPreviousSiblingRefererBy(Source, currentNode);									 \
		else if (IsRefererLessThanOtherRefererBySourceCore(currentNode, refererLinkerIndex, refererTargetIndex))			 \
			currentNode = _GetNextSiblingRefererBy(Source, currentNode);										 \
		else																									 \
			return currentNode;																					 \
																												 \
	return null;																								 \
}

#define DefineSearchInTreeOfReferersByLinkerMethod()															 \
link_index SearchRefererOfLinker(link_index linkIndex, link_index refererSourceIndex, link_index refererTargetIndex)								 \
{																												 \
	link_index currentNode = _GetFirstRefererBy(Linker, of(linkIndex));													 \
																												 \
	while (currentNode)																							 \
		if (IsRefererGreaterThanOtherRefererByLinkerCore(currentNode, refererSourceIndex, refererTargetIndex))			 \
			currentNode = _GetPreviousSiblingRefererBy(Linker, currentNode);									 \
		else if (IsRefererLessThanOtherRefererByLinkerCore(currentNode, refererSourceIndex, refererTargetIndex))			 \
			currentNode = _GetNextSiblingRefererBy(Linker, currentNode);										 \
		else																									 \
			return currentNode;																					 \
																												 \
	return null;																								 \
}

#define DefineSearchInTreeOfReferersByTargetMethod()															 \
link_index SearchRefererOfTarget(link_index linkIndex, link_index refererSourceIndex, link_index refererLinkerIndex)								 \
{																												 \
	link_index currentNode = _GetFirstRefererBy(Target, of(linkIndex));													 \
																												 \
	while (currentNode)																							 \
		if (IsRefererGreaterThanOtherRefererByTargetCore(currentNode, refererLinkerIndex, refererSourceIndex))			 \
			currentNode = _GetPreviousSiblingRefererBy(Target, currentNode);									 \
		else if (IsRefererLessThanOtherRefererByTargetCore(currentNode, refererLinkerIndex, refererSourceIndex))			 \
			currentNode = _GetNextSiblingRefererBy(Target, currentNode);										 \
		else																									 \
			return currentNode;																					 \
																												 \
	return null;																								 \
}

// OK
#define DefineAllSearchMethods() \
	DefineSearchInTreeOfReferersBySourceMethod() \
	DefineSearchInListOfReferersByLinkerMethod() \
	DefineSearchInTreeOfReferersByTargetMethod()

#endif
