#ifndef __LINKS_LINK_H__
#define __LINKS_LINK_H__

#define itself 0

#if defined(_MFC_VER)
/* none */
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
#if defined(_MFC_VER)
	unsigned long long ReferersBySourceCount; // Количество связей ссылающихся на эту связь в качестве начальной связи (количество элементов в дереве)
	unsigned long long ReferersByLinkerCount; // Количество связей ссылающихся на эту связь в качестве связи связки (количество элементов в дереве)
	unsigned long long ReferersByTargetCount; // Количество связей ссылающихся на эту связь в качестве конечной связи (количество элементов в дереве)
#elif defined(__GNUC__)
	uint64_t ReferersBySourceCount; // Количество связей ссылающихся на эту связь в качестве начальной связи (количество элементов в дереве)
	uint64_t ReferersByLinkerCount; // Количество связей ссылающихся на эту связь в качестве связи связки (количество элементов в дереве)
	uint64_t ReferersByTargetCount; // Количество связей ссылающихся на эту связь в качестве конечной связи (количество элементов в дереве)
#endif

#if defined(_MFC_VER)
	long long Timestamp; // Не использутся
#elif defined(__GNUC__)
	int64_t Timestamp;
#endif
} Link;

typedef int (*func)(Link *); // callback
typedef void (*action)(Link *); // callback


#if defined(__cplusplus)
extern "C" {
#endif

// see http://stackoverflow.com/questions/538134/exporting-functions-from-a-dll-with-dllexport
#if defined(_WIN32)
#if defined(LINKS_DLL)
#define PREFIX_DLL __stdcall __declspec(dllexport)
#else
#define PREFIX_DLL __stdcall __declspec(dllimport)
#endif
// Linux,Unix
#else
#define PREFIX_DLL 
#endif

Link* PREFIX_DLL CreateLink(Link* source, Link* linker, Link* target);
Link* PREFIX_DLL UpdateLink(Link* link, Link* source, Link* linker, Link* target);
void  PREFIX_DLL DeleteLink(Link* link);
Link* PREFIX_DLL ReplaceLink(Link* link, Link* replacement);
Link* PREFIX_DLL SearchLink(Link* source, Link* linker, Link* target);

#if defined(_MFC_VER)
unsigned long long PREFIX_DLL GetLinkNumberOfReferersBySource(Link *link);
unsigned long long PREFIX_DLL GetLinkNumberOfReferersByLinker(Link *link);
unsigned long long PREFIX_DLL GetLinkNumberOfReferersByTarget(Link *link);
#elif defined(__GNUC__)
uint64_t PREFIX_DLL GetLinkNumberOfReferersBySource(Link *link);
uint64_t PREFIX_DLL GetLinkNumberOfReferersByLinker(Link *link);
uint64_t PREFIX_DLL GetLinkNumberOfReferersByTarget(Link *link);
#endif

void PREFIX_DLL WalkThroughAllReferersBySource(Link* root, action);
int PREFIX_DLL WalkThroughReferersBySource(Link* root, func);

void PREFIX_DLL WalkThroughAllReferersByLinker(Link* root, action);
int PREFIX_DLL WalkThroughReferersByLinker(Link* root, func);

void PREFIX_DLL WalkThroughAllReferersByTarget(Link* root, action);
int PREFIX_DLL WalkThroughReferersByTarget(Link* root, func);

// not exported

void AttachLinkToMarker(Link *link, Link *marker);
void DetachLinkFromMarker(Link* link, Link* marker);

void DetachLink(Link* link);

#if defined(__cplusplus)
}
#endif

#endif
