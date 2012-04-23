#ifndef __LINKS_LINK_H__
#define __LINKS_LINK_H__

#define itself 0

#include "Common.h"


typedef struct Link
{
	struct Link *Source; // Ссылка на начальную связь
	struct Link *Linker; // Ссылка на связь связку
	struct Link *Target; // Ссылка на конечную связь
	struct Link *FirstRefererBySource; // Ссылка на вершину дерева связей ссылающихся на эту связь в качестве начальной связи
	struct Link *FirstRefererByLinker; // Ссылка на вершину дерева связей ссылающихся на эту связь в качестве связи связки
	struct Link *FirstRefererByTarget; // Ссылка на вершину дерева связей ссылающихся на эту связь в качестве конечной связи
	struct Link *NextSiblingRefererBySource; // Ссылка на правое поддерво связей ссылающихся на эту связь в качестве начальной связи
	struct Link *NextSiblingRefererByLinker; // Ссылка на правое поддерво связей ссылающихся на эту связь в качестве связи связки
	struct Link *NextSiblingRefererByTarget; // Ссылка на правое поддерво связей ссылающихся на эту связь в качестве конечной связи
	struct Link *PreviousSiblingRefererBySource; // Ссылка на левое поддерво связей ссылающихся на эту связь в качестве начальной связи
	struct Link *PreviousSiblingRefererByLinker; // Ссылка на левое поддерво связей ссылающихся на эту связь в качестве связи связки
	struct Link *PreviousSiblingRefererByTarget; // Ссылка на левое поддерво связей ссылающихся на эту связь в качестве конечной связи
	uint64_t ReferersBySourceCount; // Количество связей ссылающихся на эту связь в качестве начальной связи (количество элементов в дереве)
	uint64_t ReferersByLinkerCount; // Количество связей ссылающихся на эту связь в качестве связи связки (количество элементов в дереве)
	uint64_t ReferersByTargetCount; // Количество связей ссылающихся на эту связь в качестве конечной связи (количество элементов в дереве)
	int64_t Timestamp; // Не использутся
} Link;

typedef int (*func)(Link *); // callback
typedef void (*action)(Link *); // callback


#if defined(__cplusplus)
extern "C" {
#endif

// see http://stackoverflow.com/questions/538134/exporting-functions-from-a-dll-with-dllexport
#if defined(_WIN32)
#if defined(LINKS_DLL)
#define PREFIX_DLL __declspec(dllexport)
#else
#define PREFIX_DLL __declspec(dllimport)
#endif
// Linux,Unix
#else
#define PREFIX_DLL 
#endif

PREFIX_DLL Link* CreateLink(Link* source, Link* linker, Link* target);
//Link* PREFIX_DLL CreateLink(Link* source, Link* linker, Link* target);
PREFIX_DLL Link* UpdateLink(Link* link, Link* source, Link* linker, Link* target);
PREFIX_DLL void  DeleteLink(Link* link);
PREFIX_DLL Link* ReplaceLink(Link* link, Link* replacement);
PREFIX_DLL Link* SearchLink(Link* source, Link* linker, Link* target);

PREFIX_DLL uint64_t GetLinkNumberOfReferersBySource(Link *link);
PREFIX_DLL uint64_t GetLinkNumberOfReferersByLinker(Link *link);
PREFIX_DLL uint64_t GetLinkNumberOfReferersByTarget(Link *link);

PREFIX_DLL void WalkThroughAllReferersBySource(Link* root, action);
PREFIX_DLL int WalkThroughReferersBySource(Link* root, func);

PREFIX_DLL void WalkThroughAllReferersByLinker(Link* root, action);
PREFIX_DLL int WalkThroughReferersByLinker(Link* root, func);

PREFIX_DLL void WalkThroughAllReferersByTarget(Link* root, action);
PREFIX_DLL int WalkThroughReferersByTarget(Link* root, func);

// not exported

void AttachLinkToMarker(Link *link, Link *marker);
void DetachLinkFromMarker(Link* link, Link* marker);

void DetachLink(Link* link);

#if defined(__cplusplus)
}
#endif

#endif
