
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
#define __GetNextSiblingRefererBySource(linkIndex) _GetNextSiblingRefererBy(SourceIndex, linkIndex)
#define __GetNextSiblingRefererByLinker(linkIndex) _GetNextSiblingRefererBy(LinkerIndex, linkIndex)
#define __GetNextSiblingRefererByTarget(linkIndex) _GetNextSiblingRefererBy(TargetIndex, linkIndex)

// заменены Source на SourceIndex и т.п.
#define __SetNextSiblingRefererBySource(linkIndex, newValue) _SetNextSiblingRefererBy(SourceIndex, linkIndex, newValue)
#define __SetNextSiblingRefererByLinker(linkIndex, newValue) _SetNextSiblingRefererBy(LinkerIndex, linkIndex, newValue)
#define __SetNextSiblingRefererByTarget(linkIndex, newValue) _SetNextSiblingRefererBy(TargetIndex, linkIndex, newValue)

// заменены Source на SourceIndex и т.п.
#define __GetPreviousSiblingRefererBySource(linkIndex) _GetPreviousSiblingRefererBy(SourceIndex, linkIndex)
#define __GetPreviousSiblingRefererByLinker(linkIndex) _GetPreviousSiblingRefererBy(LinkerIndex, linkIndex)
#define __GetPreviousSiblingRefererByTarget(linkIndex) _GetPreviousSiblingRefererBy(TargetIndex, linkIndex)

// заменены Source на SourceIndex и т.п.
#define __SetPreviousSiblingRefererBySource(linkIndex, newValue) _SetPreviousSiblingRefererBy(SourceIndex, linkIndex, newValue)
#define __SetPreviousSiblingRefererByLinker(linkIndex, newValue) _SetPreviousSiblingRefererBy(LinkerIndex, linkIndex, newValue)
#define __SetPreviousSiblingRefererByTarget(linkIndex, newValue) _SetPreviousSiblingRefererBy(TargetIndex, linkIndex, newValue)

// +Index
#define __GetNumberOfReferersBySource(linkIndex) _GetNumberOfReferersBy(SourceIndex, linkIndex)
#define __GetNumberOfReferersByLinker(linkIndex) _GetNumberOfReferersBy(LinkerIndex, linkIndex)
#define __GetNumberOfReferersByTarget(linkIndex) _GetNumberOfReferersBy(TargetIndex, linkIndex)

// +Index
#define __SetNumberOfReferersBySource(linkIndex, newValue) _SetNumberOfReferersBy(SourceIndex, linkIndex, newValue)
#define __SetNumberOfReferersByLinker(linkIndex, newValue) _SetNumberOfReferersBy(LinkerIndex, linkIndex, newValue)
#define __SetNumberOfReferersByTarget(linkIndex, newValue) _SetNumberOfReferersBy(TargetIndex, linkIndex, newValue)

#define BeginWalkThroughReferersBySource(element, link) BeginWalkThroughLinksList(element, _GetFirstRefererBy(Source, link))
#define EndWalkThroughReferersBySource(element) EndWalkThroughLinksList(element, __GetNextSiblingRefererBySource)

#define BeginWalkThroughReferersByLinker(element, link) BeginWalkThroughLinksList(element, _GetFirstRefererBy(Linker, link))
#define EndWalkThroughReferersByLinker(element) EndWalkThroughLinksList(element, __GetNextSiblingRefererByLinker)

#define BeginWalkThroughReferersByTarget(element, link) BeginWalkThroughLinksList(element, _GetFirstRefererBy(Target, link))
#define EndWalkThroughReferersByTarget(element) EndWalkThroughLinksList(element, __GetNextSiblingRefererBySource)																													

#define BeginWalkThroughLinksList(element, first)																		\
{																														\
	Link* firstElement = first;																							\
	if (firstElement != null) 																							\
	{																													\
		Link* element = firstElement;																					\
		do																												\
		{																												

#define EndWalkThroughLinksList(element, nextSelector)																	\
			element = nextSelector(element);																			\
		}																												\
		while (element != firstElement);																				\
	}																													\
}

#define UnSubscribeFromListOfReferersBy(that, link, previousValue) 																		\
{																														\
	Link* nextReferer = _GetNextSiblingRefererBy(that,link);																\
																														\
	if (nextReferer != link)																							\
	{																													\
		Link* previousReferer = _GetPreviousSiblingRefererBy(that,link);													\
																														\
		_SetPreviousSiblingRefererBy(that, of(nextReferer), to(previousReferer));										\
		_SetNextSiblingRefererBy(that, of(previousReferer), to(nextReferer));											\
																														\
		if (_GetFirstRefererBy(that, of(previousValue)) == link)															\
			_SetFirstRefererBy(that, of(previousValue), to(nextReferer));												\
	}																													\
	else if (_GetFirstRefererBy(that, of(previousValue)) == link)														\
		_SetFirstRefererBy(that, of(previousValue), to(null)); 															\
																														\
	_DecrementNumberOfReferers(that, of(previousValue));																	 \
	_SetNextSiblingRefererBy(that, of(link), to(null));																		  \
	_SetPreviousSiblingRefererBy(that, of(link), to(null));																	  \
}																														

#define SubscribeToListOfReferersBy(that, link, newValue)																\
{																														\
	Link* previousFirstReferer = _GetFirstRefererBy(that, of(newValue));												\
																														\
	if (previousFirstReferer != null)																					\
	{																													\
		Link* previousLastReferer = _GetPreviousSiblingRefererBy(that, of(previousFirstReferer));						\
																														\
		_SetNextSiblingRefererBy(that, of(link), to(previousFirstReferer));												\
		_SetPreviousSiblingRefererBy(that, of(previousFirstReferer), to(link));											\
																														\
		_SetPreviousSiblingRefererBy(that, of(link), to(previousLastReferer));											\
		_SetNextSiblingRefererBy(that, of(previousLastReferer), to(link));												\
	}																													\
	else																												\
	{																													\
		_SetNextSiblingRefererBy(that, of(link), to(link));																\
		_SetPreviousSiblingRefererBy(that, of(link), to(link));															\
	}																													\
																														\
	_SetFirstRefererBy(that, of(newValue), to(link));																	\
																														\
	_IncrementNumberOfReferers(that, of(newValue));																		\
}

// +Index
#define IsRefererLessThanOtherRefererBySourceCore(refererIndex, otherRefererLinkerIndex, otherRefererTargetIndex) (GetLinkerIndex(refererIndex) < (otherRefererLinkerIndex) || (GetLinkerIndex(refererIndex) == (otherRefererLinkerIndex) && GetTargetIndex(refererIndex) < (otherRefererTargetIndex)))
#define IsRefererGreaterThanOtherRefererBySourceCore(refererIndex, otherRefererLinkerIndex, otherRefererTargetIndex) (GetLinkerIndex(refererIndex) > (otherRefererLinkerIndex) || (GetLinkerIndex(refererIndex) == (otherRefererLinkerIndex) && GetTargetIndex(refererIndex) > (otherRefererTargetIndex)))
// +Index
#define IsRefererLessThanOtherRefererBySource(refererIndex, otherRefererIndex) IsRefererLessThanOtherRefererBySourceCore(refererIndex, GetLinkerIndex(otherReferer), GetTargetIndex(otherRefererIndex))
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


#define DefineReferersTreeLeftRotateMethod(referesToThat) \
	DefineTreeLeftRotateMethod( \
		Concat3(ReferersBy,referesToThat,TreeLeftRotate), \
		struct Link, \
		Concat(__GetPreviousSiblingRefererBy,referesToThat),Concat(__SetPreviousSiblingRefererBy,referesToThat), \
		Concat(__GetNextSiblingRefererBy,referesToThat),Concat(__SetNextSiblingRefererBy,referesToThat), \
		Concat(__GetNumberOfReferersBy,referesToThat),Concat(__SetNumberOfReferersBy,referesToThat))

#define DefineReferersTreeRightRotateMethod(referesToThat) \
	DefineTreeRightRotateMethod( \
		Concat3(ReferersBy, referesToThat, TreeRightRotate), \
		struct Link, \
		Concat(__GetPreviousSiblingRefererBy, referesToThat), Concat(__SetPreviousSiblingRefererBy, referesToThat), \
		Concat(__GetNextSiblingRefererBy, referesToThat), Concat(__SetNextSiblingRefererBy, referesToThat), \
		Concat(__GetNumberOfReferersBy, referesToThat), Concat(__SetNumberOfReferersBy, referesToThat))

#define DefineReferersTreeMaintainMethod(referesToThat) \
	DefineTreeMaintainMethods( \
		Concat3(ReferersBy, referesToThat, TreeLeftMaintain), \
		Concat3(ReferersBy, referesToThat, TreeRightMaintain), \
		struct Link, \
		Concat3(ReferersBy, referesToThat, TreeLeftRotate), \
		Concat3(ReferersBy, referesToThat, TreeRightRotate), \
		Concat(__GetPreviousSiblingRefererBy, referesToThat), \
		Concat(__GetNextSiblingRefererBy, referesToThat), \
		Concat(__GetNumberOfReferersBy, referesToThat))

#define DefineReferersTreeInsertMethod(referesToThat) \
	DefineTreeInsertMethod(Concat3(ReferersBy, referesToThat, TreeInsert), struct Link, \
		Concat3(ReferersBy, referesToThat, TreeLeftMaintain), \
		Concat3(ReferersBy, referesToThat, TreeRightMaintain), \
		Concat(IsRefererLessThanOtherRefererBy, referesToThat), \
		Concat(__GetPreviousSiblingRefererBy, referesToThat), \
		Concat(__GetNextSiblingRefererBy, referesToThat), \
		Concat(__GetNumberOfReferersBy, referesToThat), \
		Concat(__SetNumberOfReferersBy, referesToThat))

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

#define GetNumberOfReferersInList(that, link) Concat3((link)->ReferersBy,that,Count)
#define GetNumberOfReferersInTree(that, link) _GetFirstRefererBy(that, of(link)) ? _GetNumberOfReferersBy(that, _GetFirstRefererBy(that, of(link))) : 0

#define GetNumberOfReferersBySource(link) GetNumberOfReferersInTree(Source, link)
#define GetNumberOfReferersByLinker(link) GetNumberOfReferersInList(Linker, link)
#define GetNumberOfReferersByTarget(link) GetNumberOfReferersInTree(Target, link)

#define DefineSearchInListOfReferersBySourceMethod()															 \
Link *SearchRefererOfSource(Link *link, Link *refererTarget, Link* refererLinker)								 \
{																												 \
	BeginWalkThroughReferersByTarget(referer, in(link))															 \
		if (referer->Target == refererTarget && referer->Linker == refererLinker)								 \
			return referer;																						 \
	EndWalkThroughReferersByTarget(referer);																	 \
	return null;																								 \
}

#define DefineSearchInListOfReferersByLinkerMethod()															 \
Link *SearchRefererOfLinker(Link *link, Link *refererSource, Link* refererTarget)								 \
{																												 \
	BeginWalkThroughReferersByTarget(referer, in(link))															 \
		if (referer->Source == refererSource && referer->Target == refererTarget)								 \
			return referer;																						 \
	EndWalkThroughReferersByTarget(referer);																	 \
	return null;																								 \
}

#define DefineSearchInListOfReferersByTargetMethod()															 \
Link *SearchRefererOfTarget(Link *link, Link *refererSource, Link* refererLinker)								 \
{																												 \
	BeginWalkThroughReferersByTarget(referer, in(link))															 \
		if (referer->Source == refererSource && referer->Linker == refererLinker)								 \
			return referer;																						 \
	EndWalkThroughReferersByTarget(referer);																	 \
	return null;																								 \
}

#define DefineSearchInTreeOfReferersBySourceMethod()															 \
Link *SearchRefererOfSource(Link *link, Link *refererTarget, Link* refererLinker)								 \
{																												 \
	Link *currentNode = _GetFirstRefererBy(Source, of(link));													 \
																												 \
	while (currentNode)																							 \
		if (IsRefererGreaterThanOtherRefererBySourceCore(currentNode, refererLinker, refererTarget))			 \
			currentNode = _GetPreviousSiblingRefererBy(Source, currentNode);									 \
		else if (IsRefererLessThanOtherRefererBySourceCore(currentNode, refererLinker, refererTarget))			 \
			currentNode = _GetNextSiblingRefererBy(Source, currentNode);										 \
		else																									 \
			return currentNode;																					 \
																												 \
	return null;																								 \
}

#define DefineSearchInTreeOfReferersByLinkerMethod()															 \
Link *SearchRefererOfLinker(Link *link, Link *refererSource, Link* refererTarget)								 \
{																												 \
	Link *currentNode = _GetFirstRefererBy(Linker, of(link));													 \
																												 \
	while (currentNode)																							 \
		if (IsRefererGreaterThanOtherRefererByLinkerCore(currentNode, refererSource, refererTarget))			 \
			currentNode = _GetPreviousSiblingRefererBy(Linker, currentNode);									 \
		else if (IsRefererLessThanOtherRefererByLinkerCore(currentNode, refererSource, refererTarget))			 \
			currentNode = _GetNextSiblingRefererBy(Linker, currentNode);										 \
		else																									 \
			return currentNode;																					 \
																												 \
	return null;																								 \
}

#define DefineSearchInTreeOfReferersByTargetMethod()															 \
Link *SearchRefererOfTarget(Link *link, Link *refererSource, Link* refererLinker)								 \
{																												 \
	Link *currentNode = _GetFirstRefererBy(Target, of(link));													 \
																												 \
	while (currentNode)																							 \
		if (IsRefererGreaterThanOtherRefererByTargetCore(currentNode, refererLinker, refererSource))			 \
			currentNode = _GetPreviousSiblingRefererBy(Target, currentNode);									 \
		else if (IsRefererLessThanOtherRefererByTargetCore(currentNode, refererLinker, refererSource))			 \
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

