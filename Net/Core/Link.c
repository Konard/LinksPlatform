
#include "Link.h"

#include "Common.h"

#include "PersistentMemoryManager.h"
#include "SizeBalancedTree.h"
#include "Timestamp.h"

#include "LinkLowLevel.h"

#include <malloc.h> // NULL

#ifdef _WIN32
#ifdef LINKS_DLL
#define _H __stdcall
#else
#define _H 
#endif
#else
#define _H 
#endif


DefineAllReferersTreeMethods(Source)
DefineAllReferersTreeMethods(Linker)
DefineAllReferersTreeMethods(Target)

DefineAllSearchMethods()


void _H AttachLink(uint64_t linkIndex, uint64_t sourceIndex, uint64_t linkerIndex, uint64_t targetIndex)
{
	Link *link = GetLink(linkIndex);
	link->Source = sourceIndex;
	link->Linker = linkerIndex;
	link->Target = targetIndex;

	SubscribeAsRefererToSource(linkIndex, sourceIndex);
	SubscribeAsRefererToLinker(linkIndex, linkerIndex);
	SubscribeAsRefererToTarget(linkIndex, targetIndex);
}

void _H DetachLink(uint64_t linkIndex)
{
	Link* link = GetLink(linkIndex);
	UnSubscribeFromSource(linkIndex, link->Source);
	UnSubscribeFromLinker(linkIndex, link->Linker);
	UnSubscribeFromTarget(linkIndex, link->Target);

	link->Source = LINK_0;
	link->Linker = LINK_0;
	link->Target = LINK_0;
}

void AttachLinkToUnusedMarker(uint64_t linkIndex)
{
	GetLink(linkIndex)->Linker = LINK_0; // markerIndex == 0

	SubscribeToListOfReferersBy(Linker, linkIndex, LINK_0);
}

//void AttachLinkToUnusedMarker(Link *link) {
        //link->LinkerIndex = LINK_0;
        //SubscribeToListOfReferersBy(Linker, link, pointerToUnusedMarker);
//}


void DetachLinkFromUnusedMarker(uint64_t linkIndex)
{
//	UnSubscribeFromListOfReferersBy(Linker, linkIndex, marker);
	UnSubscribeFromListOfReferersBy(Linker, linkIndex, LINK_0);
	
	GetLink(linkIndex)->Linker = LINK_0;
}

//void DetachLinkFromUnusedMarker(Link* link) {
        //UnSubscribeFromListOfReferersBy(Linker, link, pointerToUnusedMarker);
        //link->LinkerIndex = LINK_0;
//}


//Link* _H SearchLink(Link* source, Link* linker, Link* target)
uint64_t PREFIX_DLL SearchLink(uint64_t sourceIndex, uint64_t linkerIndex, uint64_t targetIndex)
{
	if (GetLinkNumberOfReferersByTarget(targetIndex) <= GetLinkNumberOfReferersBySource(sourceIndex))
		return SearchRefererOfTarget(targetIndex, sourceIndex, linkerIndex);
	else
		return SearchRefererOfSource(sourceIndex, targetIndex, linkerIndex);
}

//Link* _H CreateLink(Link* source, Link* linker, Link* target)
uint64_t PREFIX_DLL CreateLink(uint64_t sourceIndex, uint64_t linkerIndex, uint64_t targetIndex)
{
//	Link* source = GetLink(sourceIndex);
//	Link* linker = GetLink(linkerIndex);
//	Link* target = GetLink(targetIndex);

	if (sourceIndex != LINK_0 && linkerIndex != LINK_0 && targetIndex != LINK_0) // itself -> LINK_0
    {
        uint64_t linkIndex = SearchLink(sourceIndex, linkerIndex, targetIndex);
//		Link* link = GetLink(linkIndex);
        if (linkIndex == LINK_0)
        {
            linkIndex = AllocateLink();
			GetLink(linkIndex)->Timestamp = GetTimestamp();
			if (linkIndex != LINK_0)
				AttachLink(linkIndex, sourceIndex, linkerIndex, targetIndex);
        }
        return linkIndex;
    }
    else
    {
        uint64_t linkIndex = AllocateLink();
//		Link* link = GetLink(linkIndex);

//		link->Timestamp = GetTimestamp();
		GetLink(linkIndex)->Timestamp = GetTimestamp();

		if (linkIndex != LINK_0)
		{
			sourceIndex = (sourceIndex == LINK_0 ? linkIndex : sourceIndex);
			linkerIndex = (linkerIndex == LINK_0 ? linkIndex : linkerIndex);
			targetIndex = (targetIndex == LINK_0 ? linkIndex : targetIndex);

			AttachLink(linkIndex, sourceIndex, linkerIndex, targetIndex);
		}

        return linkIndex;
	}
}

//Link* _H ReplaceLink(Link* link, Link* replacement)
uint64_t PREFIX_DLL ReplaceLink(uint64_t linkIndex, uint64_t replacementIndex)
{
	uint64_t bySourceIndex = GetBySourceIndex(linkIndex);
	uint64_t byLinkerIndex = GetByLinkerIndex(linkIndex);
	uint64_t byTargetIndex = GetByTargetIndex(linkIndex);

	if (linkIndex != replacementIndex)
	{
//		Link* firstRefererBySource = link->BySource;
//		Link* firstRefererByLinker = link->ByLinker;
//		Link* firstRefererByTarget = link->ByTarget;

		while (bySourceIndex != LINK_0)
		{
			UpdateLink(bySourceIndex, replacementIndex, GetLinkerIndex(bySourceIndex), GetTargetIndex(bySourceIndex));
			bySourceIndex = GetBySourceIndex(linkIndex);
		}

		while (byLinkerIndex != LINK_0)
		{
			UpdateLink(byLinkerIndex, GetSourceIndex(byLinkerIndex), replacementIndex, GetTargetIndex(byLinkerIndex));
			byLinkerIndex = GetByLinkerIndex(linkIndex);
		}

		while (byTargetIndex != LINK_0)
		{
			UpdateLink(byTargetIndex, GetSourceIndex(byTargetIndex), GetLinkerIndex(byTargetIndex), replacementIndex);
			byTargetIndex = GetByTargetIndex(linkIndex);
		}

//		Link *link = GetLink(linkIndex);
		FreeLink(linkIndex);

		Link *replacement = GetLink(replacementIndex);
		replacement->Timestamp = GetTimestamp();
	}
	return replacementIndex;
}

uint64_t PREFIX_DLL UpdateLink(uint64_t linkIndex, uint64_t sourceIndex, uint64_t linkerIndex, uint64_t targetIndex)
//Link* _H UpdateLink(Link* link, Link* source, Link* linker, Link* target)
{
	if(GetSourceIndex(linkIndex) == sourceIndex && GetLinkerIndex(linkIndex) == linkerIndex && GetTargetIndex(linkIndex) == targetIndex)
		return linkIndex;

	Link *link = GetLink(linkIndex);
	Link *source = GetLink(sourceIndex);
	Link *linker = GetLink(linkerIndex);
	Link *target = GetLink(targetIndex);
    if (sourceIndex != LINK_0 && linkerIndex != LINK_0 && targetIndex != LINK_0) // ? itself -> LINK_0
    {
        uint64_t existingLinkIndex = SearchLink(sourceIndex, linkerIndex, targetIndex);
        Link* existingLink = GetLink(existingLinkIndex);
        if (existingLink == NULL)
        {
			DetachLink(linkIndex);
			AttachLink(linkIndex, sourceIndex, linkerIndex, targetIndex);

			link->Timestamp = GetTimestamp();

			return linkIndex;
        }
		else
		{
			return ReplaceLink(linkIndex, existingLinkIndex);
		}
    }
    else
    {
		source = (source == itself ? link : source);
		linker = (linker == itself ? link : linker);
		target = (target == itself ? link : target);

		DetachLink(linkIndex);
		AttachLink(linkIndex, sourceIndex, linkerIndex, targetIndex);

		link->Timestamp = GetTimestamp();

        return linkIndex;
	}
}

void _H DeleteLink(uint64_t linkIndex)
{
	FreeLink(linkIndex);
}

uint64_t _H GetLinkNumberOfReferersBySource(uint64_t linkIndex) { return GetNumberOfReferersBySource(linkIndex); }
uint64_t _H GetLinkNumberOfReferersByLinker(uint64_t linkIndex) { return GetNumberOfReferersByLinker(linkIndex); }
uint64_t _H GetLinkNumberOfReferersByTarget(uint64_t linkIndex) { return GetNumberOfReferersByTarget(linkIndex); }

void WalkThroughAllReferersBySourceCore(uint64_t rootIndex, action action_)
{
	if (rootIndex != LINK_0)
	{
		WalkThroughAllReferersBySourceCore(GetLink(rootIndex)->LeftBySource, action_);
		action_(rootIndex);
		WalkThroughAllReferersBySourceCore(GetLink(rootIndex)->RightBySource, action_);
	}
}

int WalkThroughReferersBySourceCore(uint64_t rootIndex, func func_)
{
	if (rootIndex != LINK_0)
	{
		if(!WalkThroughReferersBySourceCore(GetLink(rootIndex)->LeftBySource, func_)) return false;
		if(!func_(rootIndex)) return false;
		if(!WalkThroughReferersBySourceCore(GetLink(rootIndex)->RightBySource, func_)) return false;
	}
	return true;
}

void _H WalkThroughAllReferersBySource(uint64_t rootIndex, action action_)
{
	if (rootIndex != LINK_0) WalkThroughAllReferersBySourceCore(GetLink(rootIndex)->BySource, action_);
}

int _H WalkThroughReferersBySource(uint64_t rootIndex, func func_)
{
	if (rootIndex != LINK_0) return WalkThroughReferersBySourceCore(GetLink(rootIndex)->BySource, func_);
	else return true;
}

void _H WalkThroughAllReferersByLinker(uint64_t rootIndex, action action_)
{
	if(rootIndex != LINK_0)
	{
		BeginWalkThroughReferersByLinker(element, rootIndex)
		{
			action_(element);
		}
		EndWalkThroughReferersByLinker(element);
	}
}

int _H WalkThroughReferersByLinker(uint64_t rootIndex, func func_)
{
	if(rootIndex != LINK_0)
	{
		BeginWalkThroughReferersByLinker(element, rootIndex)
		{
			if(!func_(element)) return false;
		}
		EndWalkThroughReferersByLinker(element);
	}
	return true;
}

void WalkThroughAllReferersByTargetCore(uint64_t rootIndex, action action_)
{
	if (rootIndex != LINK_0)
	{
		WalkThroughAllReferersByTargetCore(GetLink(rootIndex)->LeftByTarget, action_);
		action_(rootIndex);
		WalkThroughAllReferersByTargetCore(GetLink(rootIndex)->RightByTarget, action_);
	}
}

// обход по дереву
int WalkThroughReferersByTargetCore(uint64_t rootIndex, func func_)
{
	if(rootIndex != LINK_0)
	{
		if(!WalkThroughReferersByTargetCore(GetLink(rootIndex)->LeftByTarget, func_)) return false;
		if(!func_(rootIndex)) return false;
		if(!WalkThroughReferersByTargetCore(GetLink(rootIndex)->RightByTarget, func_)) return false;
	}
	return true;
}

void _H WalkThroughAllReferersByTarget(uint64_t rootIndex, action action_)
{
	if (rootIndex != LINK_0) WalkThroughAllReferersByTargetCore(GetTargetIndex(rootIndex), action_);
}
	
int _H WalkThroughReferersByTarget(uint64_t rootIndex, func func_)
{
	if (rootIndex != LINK_0) return WalkThroughReferersByTargetCore(GetTargetIndex(rootIndex), func_);
	else return true;
}
