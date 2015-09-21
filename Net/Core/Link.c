
// Высокоуровневая логика работы с "линками".

#include "Common.h"
#include "Timestamp.h"
#include "Link.h"
#include "PersistentMemoryManager.h"
#include "SizeBalancedTree.h"
#include "LinkLowLevel.h"

DefineAllReferersTreeMethods(Source)
DefineAllReferersTreeMethods(Linker)
DefineAllReferersTreeMethods(Target)
DefineAllSearchMethods()

link_index public_calling_convention GetSourceIndex(link_index linkIndex)
{
    return GetLink(linkIndex)->SourceIndex;
}

link_index public_calling_convention GetLinkerIndex(link_index linkIndex)
{
    return GetLink(linkIndex)->LinkerIndex;
}

link_index public_calling_convention GetTargetIndex(link_index linkIndex)
{
    return GetLink(linkIndex)->TargetIndex;
}

signed_integer public_calling_convention GetTime(link_index linkIndex)
{
    return GetLink(linkIndex)->Timestamp;
}

link_index public_calling_convention CreateLink(link_index sourceIndex, link_index linkerIndex, link_index targetIndex)
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
                AttachLink(linkIndex, sourceIndex, linkerIndex, targetIndex);
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
            AttachLink(linkIndex, sourceIndex, linkerIndex, targetIndex);
        }
        return linkIndex;
    }
}

link_index public_calling_convention SearchLink(link_index sourceIndex, link_index linkerIndex, link_index targetIndex)
{
    // смотря, какое дерево меньше (target или source); по linker - список
    if (GetNumberOfReferersByTarget(targetIndex) <= GetNumberOfReferersBySource(sourceIndex))
        return SearchRefererOfTarget(targetIndex, sourceIndex, linkerIndex);
    else
        return SearchRefererOfSource(sourceIndex, targetIndex, linkerIndex);
}

link_index public_calling_convention ReplaceLink(link_index linkIndex, link_index replacementIndex)
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

link_index public_calling_convention UpdateLink(link_index linkIndex, link_index sourceIndex, link_index linkerIndex, link_index targetIndex)
{
    Link *link = GetLink(linkIndex);
    if (link->SourceIndex == sourceIndex && link->LinkerIndex == linkerIndex && link->TargetIndex == targetIndex)
        return linkIndex;

    if (sourceIndex != itself && linkerIndex != itself && targetIndex != itself)
    {
        uint64_t existingLinkIndex = SearchLink(sourceIndex, linkerIndex, targetIndex);
        if (existingLinkIndex == null)
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
        sourceIndex = (sourceIndex == itself ? linkIndex : sourceIndex);
        linkerIndex = (linkerIndex == itself ? linkIndex : linkerIndex);
        targetIndex = (targetIndex == itself ? linkIndex : targetIndex);

        DetachLink(linkIndex);
        AttachLink(linkIndex, sourceIndex, linkerIndex, targetIndex);

        link->Timestamp = GetTimestamp();

        return linkIndex;
    }
}

void public_calling_convention DeleteLink(link_index linkIndex)
{
    GetLink(linkIndex)->Timestamp = 0;
    FreeLink(linkIndex);
}

link_index public_calling_convention GetFirstRefererBySourceIndex(link_index linkIndex)
{
    return GetLink(linkIndex)->BySourceRootIndex;
}

link_index public_calling_convention GetFirstRefererByLinkerIndex(link_index linkIndex)
{
    return GetLink(linkIndex)->ByLinkerRootIndex;
}

link_index public_calling_convention GetFirstRefererByTargetIndex(link_index linkIndex)
{
    return GetLink(linkIndex)->ByTargetRootIndex;
}

unsigned_integer public_calling_convention GetLinkNumberOfReferersBySource(link_index linkIndex) { return GetNumberOfReferersBySource(linkIndex); }
unsigned_integer public_calling_convention GetLinkNumberOfReferersByLinker(link_index linkIndex) { return GetNumberOfReferersByLinker(linkIndex); }
unsigned_integer public_calling_convention GetLinkNumberOfReferersByTarget(link_index linkIndex) { return GetNumberOfReferersByTarget(linkIndex); }

void WalkThroughAllReferersBySourceCore(link_index rootIndex, visitor visitor)
{
    if (rootIndex != null)
    {
        Link* root = GetLink(rootIndex);
        WalkThroughAllReferersBySourceCore(root->BySourceLeftIndex, visitor);
        visitor(rootIndex);
        WalkThroughAllReferersBySourceCore(root->BySourceRightIndex, visitor);
    }
}

int WalkThroughReferersBySourceCore(link_index rootIndex, stoppable_visitor stoppableVisitor)
{
    if (rootIndex != null)
    {
        Link* root = GetLink(rootIndex);
        if (!WalkThroughReferersBySourceCore(root->BySourceLeftIndex, stoppableVisitor)) return false;
        if (!stoppableVisitor(rootIndex)) return false;
        if (!WalkThroughReferersBySourceCore(root->BySourceRightIndex, stoppableVisitor)) return false;
    }
    return true;
}

void public_calling_convention WalkThroughAllReferersBySource(link_index rootIndex, visitor visitor)
{
    if (rootIndex != null) WalkThroughAllReferersBySourceCore(GetLink(rootIndex)->BySourceRootIndex, visitor);
}

signed_integer public_calling_convention WalkThroughReferersBySource(link_index rootIndex, stoppable_visitor stoppableVisitor)
{
    if (rootIndex != null) return WalkThroughReferersBySourceCore(GetLink(rootIndex)->BySourceRootIndex, stoppableVisitor);
    else return true;
}

void public_calling_convention WalkThroughAllReferersByLinker(link_index rootIndex, visitor visitor)
{
    if (rootIndex != null)
    {
        BeginWalkThroughReferersByLinker(element, rootIndex)
        {
            visitor(element);
        }
        EndWalkThroughReferersByLinker(element);
    }
}

signed_integer public_calling_convention WalkThroughReferersByLinker(link_index rootIndex, stoppable_visitor stoppableVisitor)
{
    if (rootIndex != null)
    {
        BeginWalkThroughReferersByLinker(element, rootIndex)
        {
            if (!stoppableVisitor(element)) return false;
        }
        EndWalkThroughReferersByLinker(element);
    }
    return true;
}

void WalkThroughAllReferersByTargetCore(link_index rootIndex, visitor visitor)
{
    if (rootIndex != null)
    {
        Link* root = GetLink(rootIndex);
        WalkThroughAllReferersByTargetCore(root->ByTargetLeftIndex, visitor);
        visitor(rootIndex);
        WalkThroughAllReferersByTargetCore(root->ByTargetRightIndex, visitor);
    }
}

int WalkThroughReferersByTargetCore(link_index rootIndex, stoppable_visitor stoppableVisitor)
{
    if (rootIndex != null)
    {
        Link* root = GetLink(rootIndex);
        if (!WalkThroughReferersByTargetCore(root->ByTargetLeftIndex, stoppableVisitor)) return false;
        if (!stoppableVisitor(rootIndex)) return false;
        if (!WalkThroughReferersByTargetCore(root->ByTargetRightIndex, stoppableVisitor)) return false;
    }
    return true;
}

void public_calling_convention WalkThroughAllReferersByTarget(link_index rootIndex, visitor visitor)
{
    if (rootIndex != null) WalkThroughAllReferersByTargetCore(GetLink(rootIndex)->ByTargetRootIndex, visitor);
}

signed_integer public_calling_convention WalkThroughReferersByTarget(link_index rootIndex, stoppable_visitor stoppableVisitor)
{
    if (rootIndex != null) return WalkThroughReferersByTargetCore(GetLink(rootIndex)->ByTargetRootIndex, stoppableVisitor);
    else return true;
}

void AttachLink(link_index linkIndex, uint64_t sourceIndex, uint64_t linkerIndex, uint64_t targetIndex)
{
    Link* link = GetLink(linkIndex);

    link->SourceIndex = sourceIndex;
    link->LinkerIndex = linkerIndex;
    link->TargetIndex = targetIndex;

    SubscribeAsRefererToSource(linkIndex, sourceIndex);
    SubscribeAsRefererToLinker(linkIndex, linkerIndex);
    SubscribeAsRefererToTarget(linkIndex, targetIndex);
}

void DetachLink(link_index linkIndex)
{
    Link* link = GetLink(linkIndex);

    UnSubscribeFromSource(linkIndex, link->SourceIndex);
    UnSubscribeFromLinker(linkIndex, link->LinkerIndex);
    UnSubscribeFromTarget(linkIndex, link->TargetIndex);

    link->SourceIndex = null;
    link->LinkerIndex = null;
    link->TargetIndex = null;
}

void AttachLinkToUnusedMarker(link_index linkIndex)
{
    SubscribeToListOfReferersBy(Linker, linkIndex, null);
}

void DetachLinkFromUnusedMarker(link_index linkIndex)
{
    UnSubscribeFromListOfReferersBy(Linker, linkIndex, null);
}