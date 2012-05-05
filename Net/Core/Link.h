#ifndef __LINKS_LINK_H__
#define __LINKS_LINK_H__

// ��������������� ������ ������ � "�������".

#include "Common.h"

typedef struct Link
{
        uint64_t SourceIndex; // Ссылка на начальную связь
        uint64_t LinkerIndex; // Ссылка на связь-связку
        uint64_t TargetIndex; // Ссылка на конечную связь
        uint64_t BySourceRootIndex; // Ссылка на вершину дерева связей ссылающихся на эту связь в качестве начальной связи
        uint64_t ByLinkerRootIndex; // Ссылка на вершину дерева связей ссылающихся на эту связь в качестве связи связки
        uint64_t ByTargetRootIndex; // Ссылка на вершину дерева связей ссылающихся на эту связь в качестве конечной связи
        uint64_t BySourceRightIndex; // Ссылка на правое поддерво связей ссылающихся на эту связь в качестве начальной связи
        uint64_t ByLinkerRightIndex; // Ссылка на правое поддерво связей ссылающихся на эту связь в качестве связи связки
        uint64_t ByTargetRightIndex; // Ссылка на правое поддерво связей ссылающихся на эту связь в качестве конечной связи
        uint64_t BySourceLeftIndex; // Ссылка на левое поддерво связей ссылающихся на эту связь в качестве начальной связи
        uint64_t ByLinkerLeftIndex; // Ссылка на левое поддерво связей ссылающихся на эту связь в качестве связи связки
        uint64_t ByTargetLeftIndex; // Ссылка на левое поддерво связей ссылающихся на эту связь в качестве конечной связи
        uint64_t BySourceCount; // Количество связей ссылающихся на эту связь в качестве начальной связи (элементов в дереве)
        uint64_t ByLinkerCount; // Количество связей ссылающихся на эту связь в качестве связи связки (элементов в дереве)
        uint64_t ByTargetCount; // Количество связей ссылающихся на эту связь в качестве конечной связи (элементов в дереве)
        int64_t Timestamp; // Не использутся
} Link;

typedef int (*func)(Link *); // callback
typedef void (*action)(Link *); // callback

#if defined(__cplusplus)
extern "C" {
#endif

// see http://stackoverflow.com/questions/538134/exporting-functions-from-a-dll-with-dllexport
//#if defined(_WIN32)
#if defined(_MSC_VER) || defined(__MINGW32__) || defined(__MINGW64__)
#if defined(LINKS_DLL_EXPORT)
#define PREFIX_DLL __declspec(dllexport)
#else
#define PREFIX_DLL __declspec(dllimport)
#endif
#elif defined(__linux__)
// Linux,Unix
#define PREFIX_DLL 
#endif

/*
PREFIX_DLL Link* CreateLink(Link* source, Link* linker, Link* target);
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
*/

#if defined(__cplusplus)
}
#endif

#endif
