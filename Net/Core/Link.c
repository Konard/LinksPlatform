
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

void DetachLink(Link* link)
{
	UnSubscribeFromSource(link, link->SourceIndex);
	UnSubscribeFromLinker(link, link->LinkerIndex);
	UnSubscribeFromTarget(link, link->TargetIndex);

	link->Source = null;
	link->Linker = null;
	link->Target = null;
}

////void AttachLinkToUnusedMarker(uint64_t linkIndex)
////{
////	GetLink(linkIndex)->LinkerIndex = 0; // markerIndex == 0
////
////	SubscribeToListOfReferersBy(Linker, link, marker);
////}

void AttachLinkToUnusedMarker(Link *link) {
        link->LinkerIndex = LINK_0;
        SubscribeToListOfReferersBy(Linker, link, pointerToUnusedMarker);
}


////void DetachLinkFromUnusedMarker(uint64_t linkIndex)
////{
////	UnSubscribeFromListOfReferersBy(Linker, link, marker);
////
////	link->Linker = null;
////}

void DetachLinkFromUnusedMarker(Link* link) {
        UnSubscribeFromListOfReferersBy(Linker, link, pointerToUnusedMarker);
        link->LinkerIndex = LINK_0;
}


Link* _H SearchLink(Link* source, Link* linker, Link* target)
{
	if (GetLinkNumberOfReferersByTarget(target) <= GetLinkNumberOfReferersBySource(source))
		return SearchRefererOfTarget(target, source, linker);
	else
		return SearchRefererOfSource(source, target, linker);
}

Link* _H CreateLink(Link* source, Link* linker, Link* target)
{
    if (source != itself && linker != itself && target != itself)
    {
        Link* link = SearchLink(source, linker, target);
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

Link* _H ReplaceLink(Link* link, Link* replacement)
{
	if (link != replacement)
	{
		Link* firstRefererBySource = link->FirstRefererBySource;
		Link* firstRefererByLinker = link->FirstRefererByLinker;
		Link* firstRefererByTarget = link->FirstRefererByTarget;

		while (firstRefererBySource != null)
		{
			UpdateLink(firstRefererBySource, replacement, firstRefererBySource->Linker, firstRefererBySource->Target);
			firstRefererBySource = link->FirstRefererBySource;
		}

		while (firstRefererByLinker != null)
		{
			UpdateLink(firstRefererByLinker, firstRefererByLinker->Source, replacement, firstRefererByLinker->Target);
			firstRefererByLinker = link->FirstRefererByLinker;
		}

		while (firstRefererByTarget != null)
		{
			UpdateLink(firstRefererByTarget, firstRefererByTarget->Source, firstRefererByTarget->Linker, replacement);
			firstRefererByTarget = link->FirstRefererByTarget;
		}

		FreeLink(link);

		replacement->Timestamp = GetTimestamp();
	}
	return replacement;
}

Link* _H UpdateLink(Link* link, Link* source, Link* linker, Link* target)
{
	if(link->Source == source && link->Linker == linker && link->Target == target)
		return link;

    if (source != itself && linker != itself && target != itself)
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

void _H DeleteLink(Link* link)
{
	FreeLink(link);
}

uint64_t _H GetLinkNumberOfReferersBySource(Link *link) { return GetNumberOfReferersBySource(link); }
uint64_t _H GetLinkNumberOfReferersByLinker(Link *link) { return GetNumberOfReferersByLinker(link); }
uint64_t _H GetLinkNumberOfReferersByTarget(Link *link) { return GetNumberOfReferersByTarget(link); }

void WalkThroughAllReferersBySourceCore(Link* root, action a)
{
	if (root != null)
	{
		WalkThroughAllReferersBySourceCore(root->PreviousSiblingRefererBySource, a);
		a(root);
		WalkThroughAllReferersBySourceCore(root->NextSiblingRefererBySource, a);
	}
}

int WalkThroughReferersBySourceCore(Link* root, func f)
{
	if (root != null)
	{
		if(!WalkThroughReferersBySourceCore(root->PreviousSiblingRefererBySource, f)) return false;
		if(!f(root)) return false;
		if(!WalkThroughReferersBySourceCore(root->NextSiblingRefererBySource, f)) return false;
	}
	return true;
}

void _H WalkThroughAllReferersBySource(uint64_t rootIndex, action action_)
{
	if (rootIndex != LINK_0) WalkThroughAllReferersBySourceCore(GetLink(rootIndex)->BySourceIndex, action_);
}

int _H WalkThroughReferersBySource(uint64_t rootIndex, func func_)
{
	if (rootIndex != LINK_0) return WalkThroughReferersBySourceCore(GetLink(rootIndex)->BySourceIndex, func_);
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
		WalkThroughAllReferersByTargetCore(GetLink(rootIndex)->LeftByTargetIndex, action_);
		action_(rootIndex);
		WalkThroughAllReferersByTargetCore(GetLink(rootIndex)->RightByTargetIndex, action_);
	}
}

// обход по дереву
int WalkThroughReferersByTargetCore(uint64_t rootIndex, func func_)
{
	if(rootIndex != LINK_0)
	{
		if(!WalkThroughReferersByTargetCore(GetLink(rootIndex)->LeftByTargetIndex, func_)) return false;
		if(!func_(rootIndex)) return false;
		if(!WalkThroughReferersByTargetCore(GetLink(rootIndex)->RightByTargetIndex, func_)) return false;
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
