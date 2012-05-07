
// Высокоуровневая логика работы с "линками".

#include "Common.h"
#include "Timestamp.h"
#include "Link.h"
#include "PersistentMemoryManager.h"

//#ifdef _WIN32
#if defined(_MSC_VER) || defined(__MINGW32__) || defined(__MINGW64__)
#ifdef LINKS_DLL_EXPORT
#define _H __stdcall
#else
#define _H 
#endif
#elif defined(__linux__)
#define _H 
#endif


//DefineAllReferersTreeMethods(Source)
//DefineAllReferersTreeMethods(Linker)
//DefineAllReferersTreeMethods(Target)
//DefineAllSearchMethods()

// подчиненная CreateLink/UpdateLink/.. функция

void AttachLink(Link *link, uint64_t sourceIndex, uint64_t linkerIndex, uint64_t targetIndex)
{
	link->SourceIndex = sourceIndex;
	link->LinkerIndex = linkerIndex;
	link->TargetIndex = targetIndex;

//	SubscribeAsRefererToSource(link, sourceIndex);
//	SubscribeAsRefererToLinker(link, linkerIndex);
//	SubscribeAsRefererToTarget(link, targetIndex);
}

uint64_t _H CreateLink(uint64_t sourceIndex, uint64_t linkerIndex, uint64_t targetIndex)
{
	if (sourceIndex != itself &&
		linkerIndex != itself &&
		targetIndex != itself)
	{
		uint64_t linkIndex = SearchLink(sourceIndex, linkerIndex, targetIndex);
		if (linkIndex == null)
		{
			linkIndex = AllocateLink();
			if (linkIndex != null)
			{
				Link *link = GetLink(linkIndex);
				link->Timestamp = GetTimestamp();
				AttachLink(link, sourceIndex, linkerIndex, targetIndex);
			}
		}
		return linkIndex;
	}
	else
	{
		uint64_t linkIndex = AllocateLink();
		if (linkIndex != null)
		{
			Link* link = GetLink(linkIndex);
			link->Timestamp = GetTimestamp();
			sourceIndex = (sourceIndex == itself ? linkIndex : sourceIndex);
			linkerIndex = (linkerIndex == itself ? linkIndex : linkerIndex);
			targetIndex = (targetIndex == itself ? linkIndex : targetIndex);
			AttachLink(link, sourceIndex, linkerIndex, targetIndex);
		}
		return linkIndex;
	}
}

void DetachLink(Link* link)
{
//	UnSubscribeFromSource(link, link->SourceIndex);
//	UnSubscribeFromLinker(link, link->LinkerIndex);
//	UnSubscribeFromTarget(link, link->TargetIndex);

	link->SourceIndex = null;
	link->LinkerIndex = null;
	link->TargetIndex = null;
}

/*

void AttachLinkToMarker(Link *link, Link *marker)
{
	link->Linker = marker;

	SubscribeToListOfReferersBy(Linker, link, marker);
}

void DetachLinkFromMarker(Link* link, Link* marker)
{
	UnSubscribeFromListOfReferersBy(Linker, link, marker);

	link->Linker = null;
}

Link* _H SearchLink(Link* source, Link* linker, Link* target)
{
	if (GetLinkNumberOfReferersByTarget(target) <= GetLinkNumberOfReferersBySource(source))
		return SearchRefererOfTarget(target, source, linker);
	else
		return SearchRefererOfSource(source, target, linker);
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

void _H WalkThroughAllReferersBySource(Link* root, action a)
{
	if (root != null) WalkThroughAllReferersBySourceCore(root->FirstRefererBySource, a);
}
	
int _H WalkThroughReferersBySource(Link* root, func f)
{
	if (root != null) return WalkThroughReferersBySourceCore(root->FirstRefererBySource, f);
	else return true;
}

void _H WalkThroughAllReferersByLinker(Link* root, action a)
{
	if(root != null)
	{
		BeginWalkThroughReferersByLinker(element, root)
		{
			a(element);
		}
		EndWalkThroughReferersByLinker(element);
	}
}

int _H WalkThroughReferersByLinker(Link* root, func f)
{
	if(root != null)
	{
		BeginWalkThroughReferersByLinker(element, root)
		{
			if(!f(element)) return false;
		}
		EndWalkThroughReferersByLinker(element);
	}
	return true;
}

void WalkThroughAllReferersByTargetCore(Link* root, action a)
{
	if (root != null)
	{
		WalkThroughAllReferersByTargetCore(root->PreviousSiblingRefererByTarget, a);
		a(root);
		WalkThroughAllReferersByTargetCore(root->NextSiblingRefererByTarget, a);
	}
}
	
int WalkThroughReferersByTargetCore(Link* root, func f)
{
	if(root != null)
	{
		if(!WalkThroughReferersByTargetCore(root->PreviousSiblingRefererByTarget, f)) return false;
		if(!f(root)) return false;
		if(!WalkThroughReferersByTargetCore(root->NextSiblingRefererByTarget, f)) return false;
	}
	return true;
}

void _H WalkThroughAllReferersByTarget(Link* root, action a)
{
	if (root != null) WalkThroughAllReferersByTargetCore(root->FirstRefererByTarget, a);
}
	
int _H WalkThroughReferersByTarget(Link* root, func f)
{
	if (root != null) return WalkThroughReferersByTargetCore(root->FirstRefererByTarget, f);
	else return true;
}
*/
