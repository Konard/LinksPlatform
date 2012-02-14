#include "Link.h"
#include "Common.h"
#include "PersistentMemoryManager.h"
#include "SizeBalancedTree.h"
#include "Time.h"

#include "Link.LowLevel.h"

DefineAllReferersTreeMethods(Source);
DefineAllReferersTreeMethods(Linker);
DefineAllReferersTreeMethods(Target);

DefineAllSearchMethods();

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
	UnSubscribeFromSource(link, link->Source);
	UnSubscribeFromLinker(link, link->Linker);
	UnSubscribeFromTarget(link, link->Target);

	link->Source = null;
	link->Linker = null;
	link->Target = null;
}

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

Link* SearchLink(Link* source, Link* linker, Link* target)
{
	if (GetLinkNumberOfReferersByTarget(target) <= GetLinkNumberOfReferersBySource(source))
		return SearchRefererOfTarget(target, source, linker);
	else
		return SearchRefererOfSource(source, target, linker);
}

Link* CreateLink(Link* source, Link* linker, Link* target)
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

Link* ReplaceLink(Link* link, Link* replacement)
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

Link* UpdateLink(Link* link, Link* source, Link* linker, Link* target)
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

void DeleteLink(Link* link)
{
	FreeLink(link);
}

unsigned long long GetLinkNumberOfReferersBySource(Link *link) { return GetNumberOfReferersBySource(link); }
unsigned long long GetLinkNumberOfReferersByLinker(Link *link) { return GetNumberOfReferersByLinker(link); }
unsigned long long GetLinkNumberOfReferersByTarget(Link *link) { return GetNumberOfReferersByTarget(link); }

void WalkThroughAllReferersBySourceCore(Link* root, void __stdcall action(Link *))
{
	if (root != null)
	{
		WalkThroughAllReferersBySourceCore(root->PreviousSiblingRefererBySource, action);
		action(root);
		WalkThroughAllReferersBySourceCore(root->NextSiblingRefererBySource, action);
	}
}
	
int WalkThroughReferersBySourceCore(Link* root, int __stdcall func(Link *))
{
	if (root != null)
	{
		if(!WalkThroughReferersBySourceCore(root->PreviousSiblingRefererBySource, func)) return false;
		if(!func(root)) return false;
		if(!WalkThroughReferersBySourceCore(root->NextSiblingRefererBySource, func)) return false;
	}
	return true;
}

void WalkThroughAllReferersBySource(Link* root, void __stdcall action(Link *))
{
	if (root != null) WalkThroughAllReferersBySourceCore(root->FirstRefererBySource, action);
}
	
int WalkThroughReferersBySource(Link* root, int __stdcall func(Link *))
{
	if (root != null) return WalkThroughReferersBySourceCore(root->FirstRefererBySource, func);
	else return true;
}

void WalkThroughAllReferersByLinker(Link* root, void __stdcall func(Link *))
{
	if(root != null)
	{
		BeginWalkThroughReferersByLinker(element, root)
		{
			func(element);
		}
		EndWalkThroughReferersByLinker(element);
	}
}

int WalkThroughReferersByLinker(Link* root, int __stdcall func(Link *))
{
	if(root != null)
	{
		BeginWalkThroughReferersByLinker(element, root)
		{
			if(!func(element)) return false;
		}
		EndWalkThroughReferersByLinker(element);
	}
	return true;
}

void WalkThroughAllReferersByTargetCore(Link* root, void __stdcall action(Link *))
{
	if (root != null)
	{
		WalkThroughAllReferersByTargetCore(root->PreviousSiblingRefererByTarget, action);
		action(root);
		WalkThroughAllReferersByTargetCore(root->NextSiblingRefererByTarget, action);
	}
}
	
int WalkThroughReferersByTargetCore(Link* root, int __stdcall func(Link *))
{
	if(root != null)
	{
		if(!WalkThroughReferersByTargetCore(root->PreviousSiblingRefererByTarget, func)) return false;
		if(!func(root)) return false;
		if(!WalkThroughReferersByTargetCore(root->NextSiblingRefererByTarget, func)) return false;
	}
	return true;
}

void WalkThroughAllReferersByTarget(Link* root, void __stdcall action(Link *))
{
	if (root != null) WalkThroughAllReferersByTargetCore(root->FirstRefererByTarget, action);
}
	
int WalkThroughReferersByTarget(Link* root, int __stdcall func(Link *))
{
	if (root != null) return WalkThroughReferersByTargetCore(root->FirstRefererByTarget, func);
	else return true;
}