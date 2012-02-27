
#define of(x) x
#define to(x) x
#define in(x) x

#define Concat(a, b) a##b
#define Concat3(a, b, c) a##b##c

// Заменил Next на Right
// Аргумент byWhat == SourceIndex, например
// newValue - это уже индекс, а не ссылка
#define _GetNextSiblingRefererBy(byWhat, linkIndex) GetLink(linkIndex)->Concat(RightBy,byWhat)
#define _SetNextSiblingRefererBy(byWhat, linkIndex, newValue) GetLink(linkIndex)->Concat(RightBy,byWhat) = newValue

// Заменил Prev на Left
#define _GetPreviousSiblingRefererBy(byWhat, linkIndex) GetLink(linkIndex)->Concat(LeftBy,byWhat)
#define _SetPreviousSiblingRefererBy(byWhat, linkIndex, newValue) GetLink(linkIndex)->Concat(LeftBy,byWhat) = newValue

// ByLinker, например
#define _GetFirstRefererBy(byWhat, linkIndex) GetLink(linkIndex)->Concat(By,byWhat)
#define _SetFirstRefererBy(byWhat, linkIndex, newValue) GetLink(linkIndex)->Concat(By,byWhat) = newValue

// BySourceCount, например
#define _IncrementNumberOfReferers(whichRererersBy, linkIndex) Concat3(GetLink(linkIndex)->By,whichRererersBy,Count++)
#define _DecrementNumberOfReferers(whichRererersBy, linkIndex) Concat3(GetLink(linkIndex)->By,whichRererersBy,Count--)

// ByLinkerCount, например
#define _GetNumberOfReferersBy(that, linkIndex) Concat3(GetLink(linkIndex)->By,that,Count)
#define _SetNumberOfReferersBy(that, linkIndex, newValue) Concat3(GetLink(linkIndex)->By,that,Count) = newValue

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
	uint64_t firstElementIndex = firstIndex;																							\
	if (firstElementIndex != LINK_0) 																							\
	{																													\
		uint64_t elementIndex = firstElementIndex;																					\
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
	uint64_t nextRefererIndex = _GetNextSiblingRefererBy(that,linkIndex);																\
																														\
	if (nextRefererIndex != link)																							\
	{																													\
		uint64_t previousRefererIndex = _GetPreviousSiblingRefererBy(that,linkIndex);													\
																														\
		_SetPreviousSiblingRefererBy(that, of(nextRefererIndex), to(previousRefererIndex));										\
		_SetNextSiblingRefererBy(that, of(previousRefererIndex), to(nextRefererIndex));											\
																														\
		if (_GetFirstRefererBy(that, of(previousValue)) == linkIndex)															\
			_SetFirstRefererBy(that, of(previousValue), to(nextRefererIndex));												\
	}																													\
	else if (_GetFirstRefererBy(that, of(previousValue)) == linkIndex)														\
		_SetFirstRefererBy(that, of(previousValue), to(LINK_0)); 															\
																														\
	_DecrementNumberOfReferers(that, of(previousValue));																	 \
	_SetNextSiblingRefererBy(that, of(linkIndex), to(LINK_0));																		  \
	_SetPreviousSiblingRefererBy(that, of(linkIndex), to(LINK_0));																	  \
}																														

// +Index
#define SubscribeToListOfReferersBy(that, linkIndex, newValue)																\
{																														\
	uint64_t previousFirstRefererIndex = _GetFirstRefererBy(that, of(newValue));												\
																														\
	if (previousFirstRefererIndex != LINK_0)																					\
	{																													\
		uint64_t previousLastRefererIndex = _GetPreviousSiblingRefererBy(that, of(previousFirstRefererIndex));						\
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

// OK, Concat3(ReferersBy -> Concat3(By
#define DefineReferersTreeLeftRotateMethod(referesToThat) \
	DefineTreeLeftRotateMethod( \
		Concat3(By,referesToThat,TreeLeftRotate), \
		struct Link, \
		Concat(__GetPreviousSiblingRefererBy,referesToThat),Concat(__SetPreviousSiblingRefererBy,referesToThat), \
		Concat(__GetNextSiblingRefererBy,referesToThat),Concat(__SetNextSiblingRefererBy,referesToThat), \
		Concat(__GetNumberOfReferersBy,referesToThat),Concat(__SetNumberOfReferersBy,referesToThat))

// OK, Concat3(ReferersBy -> Concat3(By
#define DefineReferersTreeRightRotateMethod(referesToThat) \
	DefineTreeRightRotateMethod( \
		Concat3(By, referesToThat, TreeRightRotate), \
		struct Link, \
		Concat(__GetPreviousSiblingRefererBy, referesToThat), Concat(__SetPreviousSiblingRefererBy, referesToThat), \
		Concat(__GetNextSiblingRefererBy, referesToThat), Concat(__SetNextSiblingRefererBy, referesToThat), \
		Concat(__GetNumberOfReferersBy, referesToThat), Concat(__SetNumberOfReferersBy, referesToThat))

// OK, Concat3(ReferersBy -> Concat3(By
#define DefineReferersTreeMaintainMethod(referesToThat) \
	DefineTreeMaintainMethods( \
		Concat3(By, referesToThat, TreeLeftMaintain), \
		Concat3(By, referesToThat, TreeRightMaintain), \
		struct Link, \
		Concat3(By, referesToThat, TreeLeftRotate), \
		Concat3(By, referesToThat, TreeRightRotate), \
		Concat(__GetPreviousSiblingRefererBy, referesToThat), \
		Concat(__GetNextSiblingRefererBy, referesToThat), \
		Concat(__GetNumberOfReferersBy, referesToThat))

// OK, Concat3(ReferersBy -> Concat3(By
#define DefineReferersTreeInsertMethod(referesToThat) \
	DefineTreeInsertMethod(Concat3(By, referesToThat, TreeInsert), struct Link, \
		Concat3(By, referesToThat, TreeLeftMaintain), \
		Concat3(By, referesToThat, TreeRightMaintain), \
		Concat(IsRefererLessThanOtherRefererBy, referesToThat), \
		Concat(__GetPreviousSiblingRefererBy, referesToThat), \
		Concat(__GetNextSiblingRefererBy, referesToThat), \
		Concat(__GetNumberOfReferersBy, referesToThat), \
		Concat(__SetNumberOfReferersBy, referesToThat))

// OK
#define DefineUnsafeDetachFromReferersTreeMethod(referesToThat) \
	DefineUnsafeDetachFromTreeMethod(Concat(UnsafeDetachFromTreeOfReferersBy, referesToThat), struct Link, \
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

#define BeginWalkThroughtReferersTree(byThat, element, root) BeginWalkThroughtTree(struct Link, element, _GetFirstRefererBy(byThat, root), Concat(__GetPreviousSiblingRefererBy, byThat))
#define EndWalkThroughtReferersTree(byThat, element) EndWalkThroughtTree(element, Concat(__GetNextSiblingRefererBy, byThat))

#define BeginWalkThroughtTreeOfReferersBySource(element, root) BeginWalkThroughtReferersTree(Source, element, root)
#define EndWalkThroughtTreeOfReferersBySource(element) EndWalkThroughtReferersTree(Source, element)

#define BeginWalkThroughtTreeOfReferersByLinker(element, root) BeginWalkThroughtReferersTree(Linker, element, root)
#define EndWalkThroughtTreeOfReferersByLinker(element) EndWalkThroughtReferersTree(Linker, element)

#define BeginWalkThroughtTreeOfReferersByTarget(element, root) BeginWalkThroughtReferersTree(Target, element, root)
#define EndWalkThroughtTreeOfReferersByTarget(element) EndWalkThroughtReferersTree(Target, element)

#define SubscribeToTreeOfReferersBy(that, link, newValue) Concat3(ReferersBy, that, TreeInsert)(&_GetFirstRefererBy(that, of(newValue)), link)
#define UnSubscribeFromTreeOfReferersBy(that, link, newValue) Concat(UnsafeDetachFromTreeOfReferersBy, that)(&_GetFirstRefererBy(that, of(newValue)), link)

#define SubscribeAsRefererToSource(link, newValue) SubscribeToTreeOfReferersBy(Source, link, newValue)
#define SubscribeAsRefererToLinker(link, newValue) SubscribeToListOfReferersBy(Linker, link, newValue)
#define SubscribeAsRefererToTarget(link, newValue) SubscribeToTreeOfReferersBy(Target, link, newValue)

#define UnSubscribeFromSource(link, newValue) UnSubscribeFromTreeOfReferersBy(Source, link, newValue)
#define UnSubscribeFromLinker(link, newValue) UnSubscribeFromListOfReferersBy(Linker, link, newValue)
#define UnSubscribeFromTarget(link, newValue) UnSubscribeFromTreeOfReferersBy(Target, link, newValue)

// ReferersBy -> By
#define GetNumberOfReferersInList(that, link) GetLink(link)->Concat3(By,that,Count)
#define GetNumberOfReferersInTree(that, link) _GetFirstRefererBy(that, of(link)) ? _GetNumberOfReferersBy(that, _GetFirstRefererBy(that, of(link))) : 0

#define GetNumberOfReferersBySource(link) GetNumberOfReferersInTree(Source, link)
#define GetNumberOfReferersByLinker(link) GetNumberOfReferersInList(Linker, link)
#define GetNumberOfReferersByTarget(link) GetNumberOfReferersInTree(Target, link)

// Link * -> uint64_t
#define DefineSearchInListOfReferersBySourceMethod()															 \
uint64_t SearchRefererOfSource(uint64_t linkIndex, uint64_t refererTargetIndex, uint64_t refererLinkerIndex)								 \
{																												 \
	BeginWalkThroughReferersByTarget(referer, in(linkIndex))															 \
		if (GetTargetIndex(referer) == refererTargetIndex && GetLinkerIndex(referer) == refererLinkerIndex)								 \
			return referer;																						 \
	EndWalkThroughReferersByTarget(referer);																	 \
	return LINK_0;																								 \
}

// Link * -> uint64_t
#define DefineSearchInListOfReferersByLinkerMethod()															 \
uint64_t SearchRefererOfLinker(uint64_t linkIndex, uint64_t refererSourceIndex, uint64_t refererTargetIndex)								 \
{																												 \
	BeginWalkThroughReferersByTarget(referer, in(linkIndex))															 \
		if (GetSourceIndex(referer) == refererSourceIndex && GetTargetIndex(referer) == refererTargetIndex)								 \
			return referer;																						 \
	EndWalkThroughReferersByTarget(referer);																	 \
	return LINK_0;																								 \
}

// Link * -> uint64_t
#define DefineSearchInListOfReferersByTargetMethod()															 \
uint64_t SearchRefererOfTarget(uint64_t linkIndex, uint64_t refererSourceIndex, uint64_t refererLinkerIndex)								 \
{																												 \
	BeginWalkThroughReferersByTarget(referer, in(linkIndex))															 \
		if (GetSourceIndex(referer) == refererSourceIndex && GetLinkerIndex(referer) == refererLinkerIndex)								 \
			return referer;																						 \
	EndWalkThroughReferersByTarget(referer);																	 \
	return LINK_0;																								 \
}

#define DefineSearchInTreeOfReferersBySourceMethod()															 \
uint64_t SearchRefererOfSource(uint64_t linkIndex, uint64_t refererTargetIndex, uint64_t refererLinkerIndex)								 \
{																												 \
	uint64_t currentNode = _GetFirstRefererBy(Source, of(linkIndex));													 \
																												 \
	while (currentNode)																							 \
		if (IsRefererGreaterThanOtherRefererBySourceCore(currentNode, refererLinkerIndex, refererTargetIndex))			 \
			currentNode = _GetPreviousSiblingRefererBy(Source, currentNode);									 \
		else if (IsRefererLessThanOtherRefererBySourceCore(currentNode, refererLinkerIndex, refererTargetIndex))			 \
			currentNode = _GetNextSiblingRefererBy(Source, currentNode);										 \
		else																									 \
			return currentNode;																					 \
																												 \
	return LINK_0;																								 \
}

#define DefineSearchInTreeOfReferersByLinkerMethod()															 \
uint64_t SearchRefererOfLinker(uint64_t linkIndex, uint64_t refererSourceIndex, uint64_t refererTargetIndex)								 \
{																												 \
	uint64_t currentNode = _GetFirstRefererBy(Linker, of(linkIndex));													 \
																												 \
	while (currentNode)																							 \
		if (IsRefererGreaterThanOtherRefererByLinkerCore(currentNode, refererSourceIndex, refererTargetIndex))			 \
			currentNode = _GetPreviousSiblingRefererBy(Linker, currentNode);									 \
		else if (IsRefererLessThanOtherRefererByLinkerCore(currentNode, refererSourceIndex, refererTargetIndex))			 \
			currentNode = _GetNextSiblingRefererBy(Linker, currentNode);										 \
		else																									 \
			return currentNode;																					 \
																												 \
	return LINK_0;																								 \
}

#define DefineSearchInTreeOfReferersByTargetMethod()															 \
uint64_t SearchRefererOfTarget(uint64_t linkIndex, uint64_t refererSourceIndex, uint64_t refererLinkerIndex)								 \
{																												 \
	uint64_t currentNode = _GetFirstRefererBy(Target, of(linkIndex));													 \
																												 \
	while (currentNode)																							 \
		if (IsRefererGreaterThanOtherRefererByTargetCore(currentNode, refererLinkerIndex, refererSourceIndex))			 \
			currentNode = _GetPreviousSiblingRefererBy(Target, currentNode);									 \
		else if (IsRefererLessThanOtherRefererByTargetCore(currentNode, refererLinkerIndex, refererSourceIndex))			 \
			currentNode = _GetNextSiblingRefererBy(Target, currentNode);										 \
		else																									 \
			return currentNode;																					 \
																												 \
	return LINK_0;																								 \
}

// OK
#define DefineAllSearchMethods() \
	DefineSearchInTreeOfReferersBySourceMethod() \
	DefineSearchInListOfReferersByLinkerMethod() \
	DefineSearchInTreeOfReferersByTargetMethod()

