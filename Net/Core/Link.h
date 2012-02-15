#ifndef __LINKS_LINK_H__
#define __LINKS_LINK_H__

#define itself 0

#if defined(_MFC_VER)
#elif defined(__GNUC__)
#include <stdint.h>
#endif


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
	unsigned long long ReferersBySourceCount; // Количество связей ссылающихся на эту связь в качестве начальной связи (количество элементов в дереве)
	unsigned long long ReferersByLinkerCount; // Количество связей ссылающихся на эту связь в качестве связи связки (количество элементов в дереве)
	unsigned long long ReferersByTargetCount; // Количество связей ссылающихся на эту связь в качестве конечной связи (количество элементов в дереве)
#if defined(_MFC_VER)
	long long Timestamp; // Не использутся
#elif defined(__GNUC__)
	uint64_t Timestamp;
#endif
} Link;

__declspec(dllexport) Link* CreateLink(Link* source, Link* linker, Link* target);
__declspec(dllexport) Link* UpdateLink(Link* link, Link* source, Link* linker, Link* target);
__declspec(dllexport) void DeleteLink(Link* link);
__declspec(dllexport) Link* ReplaceLink(Link* link, Link* replacement);
__declspec(dllexport) Link* SearchLink(Link* source, Link* linker, Link* target);

__declspec(dllexport) unsigned long long GetLinkNumberOfReferersBySource(Link *link);
__declspec(dllexport) unsigned long long GetLinkNumberOfReferersByLinker(Link *link);
__declspec(dllexport) unsigned long long GetLinkNumberOfReferersByTarget(Link *link);

__declspec(dllexport) void WalkThroughAllReferersBySource(Link* root, void __stdcall action(Link *));
__declspec(dllexport) int WalkThroughReferersBySource(Link* root, int __stdcall func(Link *));

__declspec(dllexport) void WalkThroughAllReferersByLinker(Link* root, void __stdcall func(Link *));
__declspec(dllexport) int WalkThroughReferersByLinker(Link* root, int __stdcall func(Link *));

__declspec(dllexport) void WalkThroughAllReferersByTarget(Link* root, void __stdcall action(Link *));
__declspec(dllexport) int WalkThroughReferersByTarget(Link* root, int __stdcall func(Link *));

void AttachLinkToMarker(Link *link, Link *marker);
void DetachLinkFromMarker(Link* link, Link* marker);

void DetachLink(Link* link);

#endif
