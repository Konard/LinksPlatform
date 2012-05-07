
// Высокоуровневая логика работы с "линками".

#include "Common.h"
#include "Timestamp.h"
#include "Link.h"
#include "PersistentMemoryManager.h"

#if defined(_MSC_VER) || defined(__MINGW32__) || defined(__MINGW64__)
#ifdef LINKS_DLL_EXPORT
//#define _H __stdcall
#define _H 
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

void AttachLinkToUnusedMarker(Link *link)
{
	link->LinkerIndex = null;

//	SubscribeToListOfReferersBy(LinkerIndex, link, null);
}

void DetachLinkFromUnusedMarker(Link* link)
{
//	UnSubscribeFromListOfReferersBy(LinkerIndex, link, null);

	link->LinkerIndex = null;
}

uint64_t _H SearchLink(uint64_t sourceIndex, uint64_t linkerIndex, uint64_t targetIndex)
{
	// смотря, какое дерево меньше (target или source); по linker - список
/*
	if (GetLinkNumberOfReferersByTarget(targetIndex) <= GetLinkNumberOfReferersBySource(sourceIndex))
		return SearchRefererOfTarget(targetIndex, sourceIndex, linkerIndex);
	else
		return SearchRefererOfSource(sourceIndex, targetIndex, linkerIndex);
*/
}

// в функции Replace почти ничего нет - вся работа происходит в Update;
// Replace - функция-координатор

uint64_t _H ReplaceLink(uint64_t linkIndex, uint64_t replacementIndex)
{
	Link *link = GetLink(linkIndex);
	Link *replacement = GetLink(replacementIndex);

	if (linkIndex != replacementIndex)
	{
		uint64_t firstRefererBySourceIndex = link->BySourceRootIndex;
		uint64_t firstRefererByLinkerIndex = link->ByLinkerRootIndex;
		uint64_t firstRefererByTargetIndex = link->ByTargetRootIndex;

		// что здесь происходит - непонятно
		while (firstRefererBySourceIndex != null)
		{
			UpdateLink(
				firstRefererBySourceIndex,
				replacementIndex,
				GetLink(firstRefererBySourceIndex)->LinkerIndex,
				GetLink(firstRefererBySourceIndex)->TargetIndex
			);
			firstRefererBySourceIndex = link->BySourceRootIndex;
		}

		while (firstRefererByLinkerIndex != null)
		{
			UpdateLink(
				firstRefererByLinkerIndex,
				GetLink(firstRefererByLinkerIndex)->SourceIndex,
				replacementIndex,
				GetLink(firstRefererByLinkerIndex)->TargetIndex
			);
			firstRefererByLinkerIndex = link->ByLinkerRootIndex;
		}

		while (firstRefererByTargetIndex != null)
		{
			UpdateLink(
				firstRefererByTargetIndex,
				GetLink(firstRefererByTargetIndex)->SourceIndex,
				GetLink(firstRefererByTargetIndex)->LinkerIndex,
				replacementIndex
			);
			firstRefererByTargetIndex = link->ByTargetRootIndex;
		}

		FreeLink(linkIndex);

		replacement->Timestamp = GetTimestamp();
	}
	return replacementIndex;
}


uint64_t _H UpdateLink(uint64_t linkIndex, uint64_t sourceIndex, uint64_t linkerIndex, uint64_t targetIndex)
{
	Link *link = GetLink(linkIndex);
	if(link->SourceIndex == sourceIndex && link->LinkerIndex == linkerIndex && link->TargetIndex == targetIndex)
		return linkIndex;

	if (sourceIndex != itself && linkerIndex != itself && targetIndex != itself)
	{
		uint64_t existingLinkIndex = SearchLink(sourceIndex, linkerIndex, targetIndex);
		Link* existingLink = GetLink(existingLinkIndex);
		if (existingLinkIndex == null)
		{
			DetachLink(link);
			AttachLink(link, sourceIndex, linkerIndex, targetIndex);

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
		sourceIndex = (sourceIndex == itself ? linkIndex : sourceIndex);
		linkerIndex = (linkerIndex == itself ? linkIndex : linkerIndex);
		targetIndex = (targetIndex == itself ? linkIndex : targetIndex);

		DetachLink(link);
		AttachLink(link, sourceIndex, linkerIndex, targetIndex);

		link->Timestamp = GetTimestamp();

        	return linkIndex;
	}
}


void _H DeleteLink(uint64_t linkIndex)
{
	FreeLink(linkIndex);
}

/*

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
