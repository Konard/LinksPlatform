
#define of(x) x
#define to(x) x
#define in(x) x

#define Concat(a, b) a##b
#define Concat3(a, b, c) a##b##c

#define _GetNextSiblingRefererBy(byWhat, link) (link)->Concat(NextSiblingRefererBy,byWhat)
#define _SetNextSiblingRefererBy(byWhat, link, newValue) (link)->Concat(NextSiblingRefererBy,byWhat) = newValue

#define _GetPreviousSiblingRefererBy(byWhat, link) Concat((link)->PreviousSiblingRefererBy,byWhat)
#define _SetPreviousSiblingRefererBy(byWhat, link, newValue) Concat((link)->PreviousSiblingRefererBy,byWhat) = newValue

#define _GetFirstRefererBy(byWhat, link) Concat((link)->FirstRefererBy,byWhat)
#define _SetFirstRefererBy(byWhat, link, newValue) Concat((link)->FirstRefererBy,byWhat) = newValue

#define _IncrementNumberOfReferers(whichRererersBy, link) Concat3((link)->ReferersBy,whichRererersBy,Count++)
#define _DecrementNumberOfReferers(whichRererersBy, link) Concat3((link)->ReferersBy,whichRererersBy,Count--)

#define _GetNumberOfReferersBy(that, link) Concat3((link)->ReferersBy,that,Count)
#define _SetNumberOfReferersBy(that, link, newValue) Concat3((link)->ReferersBy,that,Count) = newValue

#define __GetNextSiblingRefererBySource(link) _GetNextSiblingRefererBy(Source, link)
#define __GetNextSiblingRefererByLinker(link) _GetNextSiblingRefererBy(Linker, link)
#define __GetNextSiblingRefererByTarget(link) _GetNextSiblingRefererBy(Target, link)

#define __SetNextSiblingRefererBySource(link, newValue) _SetNextSiblingRefererBy(Source, link, newValue)
#define __SetNextSiblingRefererByLinker(link, newValue) _SetNextSiblingRefererBy(Linker, link, newValue)
#define __SetNextSiblingRefererByTarget(link, newValue) _SetNextSiblingRefererBy(Target, link, newValue)

#define __GetPreviousSiblingRefererBySource(link) _GetPreviousSiblingRefererBy(Source, link)
#define __GetPreviousSiblingRefererByLinker(link) _GetPreviousSiblingRefererBy(Linker, link)
#define __GetPreviousSiblingRefererByTarget(link) _GetPreviousSiblingRefererBy(Target, link)

#define __SetPreviousSiblingRefererBySource(link, newValue) _SetPreviousSiblingRefererBy(Source, link, newValue)
#define __SetPreviousSiblingRefererByLinker(link, newValue) _SetPreviousSiblingRefererBy(Linker, link, newValue)
#define __SetPreviousSiblingRefererByTarget(link, newValue) _SetPreviousSiblingRefererBy(Target, link, newValue)

#define __GetNumberOfReferersBySource(link) _GetNumberOfReferersBy(Source, link)
#define __GetNumberOfReferersByLinker(link) _GetNumberOfReferersBy(Linker, link)
#define __GetNumberOfReferersByTarget(link) _GetNumberOfReferersBy(Target, link)

#define __SetNumberOfReferersBySource(link, newValue) _SetNumberOfReferersBy(Source, link, newValue)
#define __SetNumberOfReferersByLinker(link, newValue) _SetNumberOfReferersBy(Linker, link, newValue)
#define __SetNumberOfReferersByTarget(link, newValue) _SetNumberOfReferersBy(Target, link, newValue)

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

#define IsRefererLessThanOtherRefererBySourceCore(referer, otherRefererLinker, otherRefererTarget) ((referer)->Linker < (otherRefererLinker) || ((referer)->Linker == (otherRefererLinker) && (referer)->Target < (otherRefererTarget)))
#define IsRefererGreaterThanOtherRefererBySourceCore(referer, otherRefererLinker, otherRefererTarget) ((referer)->Linker > (otherRefererLinker) || ((referer)->Linker == (otherRefererLinker) && (referer)->Target > (otherRefererTarget)))

#define IsRefererLessThanOtherRefererBySource(referer, otherReferer) IsRefererLessThanOtherRefererBySourceCore(referer, (otherReferer)->Linker, (otherReferer)->Target)
#define IsRefererGreaterThanOtherRefererBySource(referer, otherReferer) IsRefererGreaterThanOtherRefererBySourceCore(referer, (otherReferer)->Linker, (otherReferer)->Target)

#define IsRefererLessThanOtherRefererByLinkerCore(referer, otherRefererSource, otherRefererTarget) ((referer)->Source < (otherRefererSource) || ((referer)->Source == (otherRefererSource) && (referer)->Target < (otherRefererTarget)))
#define IsRefererGreaterThanOtherRefererByLinkerCore(referer, otherRefererSource, otherRefererTarget) ((referer)->Source > (otherRefererSource) || ((referer)->Source == (otherRefererSource) && (referer)->Target > (otherRefererTarget)))

#define IsRefererLessThanOtherRefererByLinker(referer, otherReferer) IsRefererLessThanOtherRefererByLinkerCore(referer, (otherReferer)->Source, (otherReferer)->Target)
#define IsRefererGreaterThanOtherRefererByLinker(referer, otherReferer) IsRefererGreaterThanOtherRefererByLinkerCore(referer, (otherReferer)->Source, (otherReferer)->Target)

#define IsRefererLessThanOtherRefererByTargetCore(referer, otherRefererLinker, otherRefererSource) ((referer)->Linker < (otherRefererLinker) || ((referer)->Linker == (otherRefererLinker) && (referer)->Source < (otherRefererSource)))
#define IsRefererGreaterThanOtherRefererByTargetCore(referer, otherRefererLinker, otherRefererSource) ((referer)->Linker > (otherRefererLinker) || ((referer)->Linker == (otherRefererLinker) && (referer)->Source > (otherRefererSource)))

#define IsRefererLessThanOtherRefererByTarget(referer, otherReferer) IsRefererLessThanOtherRefererByTargetCore(referer, (otherReferer)->Linker, (otherReferer)->Source)
#define IsRefererGreaterThanOtherRefererByTarget(referer, otherReferer) IsRefererGreaterThanOtherRefererByTargetCore(referer, (otherReferer)->Linker, (otherReferer)->Source)

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

#define DefineAllSearchMethods() \
	DefineSearchInTreeOfReferersBySourceMethod() \
	DefineSearchInListOfReferersByLinkerMethod() \
	DefineSearchInTreeOfReferersByTargetMethod()

