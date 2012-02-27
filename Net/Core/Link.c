
#include "Link.h"

#include "Common.h"

#include "PersistentMemoryManager.h"
#include "SizeBalancedTree.h"
#include "Timestamp.h"

#include "LinkLowLevel.h"

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

void AttachLink(Link* link, Link* source, Link* linker, Link* target)
{
	link->Source = source;
	link->Linker = linker;
	link->Target = target;

	SubscribeAsRefererToSource(link, source);
	SubscribeAsRefererToLinker(link, linker);
	SubscribeAsRefererToTarget(link, target);
}

void DetachLink(uint64_t linkIndex)
{
	Link* link = GetLink(linkIndex);
	UnSubscribeFromSource(link, link->Source);
	UnSubscribeFromLinker(link, link->Linker);
	UnSubscribeFromTarget(link, link->Target);

	link->Source = LINK_0;
	link->Linker = LINK_0;
	link->Target = LINK_0;
}

void AttachLinkToUnusedMarker(uint64_t linkIndex)
{
	GetLink(linkIndex)->LinkerIndex = 0; // markerIndex == 0

	SubscribeToListOfReferersBy(Linker, link, marker);
}

//void AttachLinkToUnusedMarker(Link *link) {
        //link->LinkerIndex = LINK_0;
        //SubscribeToListOfReferersBy(Linker, link, pointerToUnusedMarker);
//}


void DetachLinkFromUnusedMarker(uint64_t linkIndex)
{
	UnSubscribeFromListOfReferersBy(Linker, link, marker);

	link->Linker = null;
}

//void DetachLinkFromUnusedMarker(Link* link) {
        //UnSubscribeFromListOfReferersBy(Linker, link, pointerToUnusedMarker);
        //link->LinkerIndex = LINK_0;
//}


//Link* _H SearchLink(Link* source, Link* linker, Link* target)
uint64_t PREFIX_DLL SearchLink(uint64_t sourceIndex, uint64_t linkerIndex, uint64_t targetIndex);
{
	if (GetLinkNumberOfReferersByTarget(targetIndex) <= GetLinkNumberOfReferersBySource(sourceIndex))
		return SearchRefererOfTarget(targetIndex, sourceIndex, linkerIndex);
	else
		return SearchRefererOfSource(sourceIndex, targetIndex, linkerIndex);
}

//Link* _H CreateLink(Link* source, Link* linker, Link* target)
uint64_t PREFIX_DLL CreateLink(uint64_t sourceIndex, uint64_t linkerIndex, uint64_t targetIndex);
{
    if (sourceIndex != LINK_0 && linkerIndex != LINK_0 && targetIndex != LINK_0) // itself -> LINK_0
    {
        uint64_t linkIndex = SearchLink(sourceIndex, linkerIndex, targetIndex);
		Link* link = GetLink(linkIndex);
        if (link == null)
        {
            link = AllocateLink();
			link->Timestamp = GetTimestamp();
			if (link != null)
				AttachLink(link, source, linker, target);
        }
        return link;
    }
    else
    {
        Link* link = AllocateLink();
		link->Timestamp = GetTimestamp();

		if (link != null)
		{
			source = (source == itself ? link : source);
			linker = (linker == itself ? link : linker);
			target = (target == itself ? link : target);

			AttachLink(link, source, linker, target);
		}

        return link;
	}
}

//Link* _H ReplaceLink(Link* link, Link* replacement)
uint64_t PREFIX_DLL ReplaceLink(uint64_t linkIndex, uint64_t replacementIndex);
{
	if (linkIndex != replacementIndex)
	{
		Link* firstRefererBySource = link->BySource;
		Link* firstRefererByLinker = link->ByLinker;
		Link* firstRefererByTarget = link->ByTarget;

		while (firstRefererBySource != null)
		{
			UpdateLink(firstRefererBySource, replacement, firstRefererBySource->Linker, firstRefererBySource->Target);
			firstRefererBySource = link->BySource;
		}

		while (firstRefererByLinker != null)
		{
			UpdateLink(firstRefererByLinker, firstRefererByLinker->Source, replacement, firstRefererByLinker->Target);
			firstRefererByLinker = link->ByLinker;
		}

		while (firstRefererByTarget != null)
		{
			UpdateLink(firstRefererByTarget, firstRefererByTarget->Source, firstRefererByTarget->Linker, replacement);
			firstRefererByTarget = link->ByTarget;
		}

		FreeLink(link);

		replacement->Timestamp = GetTimestamp();
	}
	return replacementIndex;
}

uint64_t PREFIX_DLL UpdateLink(uint64_t linkIndex, uint64_t sourceIndex, uint64_t linkerIndex, uint64_t targetIndex);
//Link* _H UpdateLink(Link* link, Link* source, Link* linker, Link* target)
{
	if(GetSourceIndex(linkIndex) == sourceIndex && GetLinkerIndex(linkIndex) == linkerIndex && GetTargetIndex(linkIndex) == targetIndex)
		return linkIndex;

    if (sourceIndex != LINK_0 && linkerIndex != LINK_0 && targetIndex != LINK_0) // ? itself -> LINK_0
    {
        Link* existingLink = SearchLink(source, linker, target);
        if (existingLink == null)
        {
			DetachLink(link);
			AttachLink(link, source, linker, target);

			link->Timestamp = GetTimestamp();

			return link;
        }
		else
		{
			return ReplaceLink(link, existingLink);
		}
    }
    else
    {
		source = (source == itself ? link : source);
		linker = (linker == itself ? link : linker);
		target = (target == itself ? link : target);

		DetachLink(link);
		AttachLink(link, source, linker, target);

		link->Timestamp = GetTimestamp();

        return link;
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
