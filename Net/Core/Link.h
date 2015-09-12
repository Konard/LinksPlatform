#ifndef __LINKS_LINK_H__
#define __LINKS_LINK_H__

// Высокоуровневая логика работы с связями.

#include "Common.h"

#define null    0LL
#define itself  0LL

typedef struct Link
{
    link_index          SourceIndex; // Ссылка на начальную связь
    link_index          TargetIndex; // Ссылка на конечную связь
    link_index          LinkerIndex; // Ссылка на связь-связку (если разместить это поле после Source и Target можно 
    /* Referers (Index, Backlinks) */
    link_index          BySourceRootIndex; // Ссылка на вершину дерева связей ссылающихся на эту связь в качестве начальной связи
    link_index          ByLinkerRootIndex; // Ссылка на вершину дерева связей ссылающихся на эту связь в качестве связи связки
    link_index          ByTargetRootIndex; // Ссылка на вершину дерева связей ссылающихся на эту связь в качестве конечной связи
    link_index          BySourceRightIndex; // Ссылка на правое поддерво связей ссылающихся на эту связь в качестве начальной связи
    link_index          ByLinkerRightIndex; // Ссылка на правое поддерво связей ссылающихся на эту связь в качестве связи связки
    link_index          ByTargetRightIndex; // Ссылка на правое поддерво связей ссылающихся на эту связь в качестве конечной связи
    link_index          BySourceLeftIndex; // Ссылка на левое поддерво связей ссылающихся на эту связь в качестве начальной связи
    link_index          ByLinkerLeftIndex; // Ссылка на левое поддерво связей ссылающихся на эту связь в качестве связи связки
    link_index          ByTargetLeftIndex; // Ссылка на левое поддерво связей ссылающихся на эту связь в качестве конечной связи
    unsigned_integer    BySourceCount; // Количество связей ссылающихся на эту связь в качестве начальной связи (элементов в дереве)
    unsigned_integer    ByLinkerCount; // Количество связей ссылающихся на эту связь в качестве связи связки (элементов в дереве)
    unsigned_integer    ByTargetCount; // Количество связей ссылающихся на эту связь в качестве конечной связи (элементов в дереве)
    signed_integer      Timestamp; // Не использутся
} Link;

typedef signed_integer(*stoppable_visitor)(link_index); // Stoppable visitor callback (Останавливаемый обработчик для прохода по связям)
typedef void(*visitor)(link_index); // Visitor callback (Неостанавливаемый обработчик для прохода по связям)

#if defined(__cplusplus)
extern "C" {
#endif

    PREFIX_DLL link_index CreateLink(link_index sourceIndex, link_index linkerIndex, link_index targetIndex);

    PREFIX_DLL link_index SearchLink(link_index sourceIndex, link_index linkerIndex, link_index targetIndex);

    PREFIX_DLL link_index ReplaceLink(link_index linkIndex, link_index replacementIndex);
    PREFIX_DLL link_index UpdateLink(link_index linkIndex, link_index sourceIndex, link_index linkerIndex, link_index targetIndex);

    PREFIX_DLL void DeleteLink(link_index linkIndex);

    /*

    PREFIX_DLL unsigned_integer GetLinkNumberOfReferersBySource(link_index linkIndex);
    PREFIX_DLL unsigned_integer GetLinkNumberOfReferersByLinker(link_index linkIndex);
    PREFIX_DLL unsigned_integer GetLinkNumberOfReferersByTarget(link_index linkIndex);

    PREFIX_DLL void WalkThroughAllReferersBySource(link_index rootIndex, visitor);
    PREFIX_DLL signed_integer WalkThroughReferersBySource(link_index rootIndex, stoppable_visitor stoppableVisitor);

    PREFIX_DLL void WalkThroughAllReferersByLinker(link_index rootIndex, visitor);
    PREFIX_DLL signed_integer WalkThroughReferersByLinker(link_index rootIndex, stoppable_visitor stoppableVisitor);

    PREFIX_DLL void WalkThroughAllReferersByTarget(link_index rootIndex, visitor);
    PREFIX_DLL signed_integer WalkThroughReferersByTarget(link_index rootIndex, stoppable_visitor stoppableVisitor);

    */

    /* "Unused marker" help mark links that was deleted, but still can be reused */

    void AttachLinkToUnusedMarker(link_index linkIndex);
    void DetachLinkFromUnusedMarker(link_index linkIndex);

    // TODO: Check if it is actually usefull
    //void DetachLink(link_index link);

#if defined(__cplusplus)
}
#endif

#endif
