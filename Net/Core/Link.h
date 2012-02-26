#ifndef __LINKS_LINK_H__
#define __LINKS_LINK_H__

#define itself 0

#include "Common.h"


typedef struct Link
{
	// заменяем всё на индексы
	uint64_t Source; // Индекс начала
	uint64_t Linker; // Индекс связки
	uint64_t Target; // Индекс конца
	//struct Link *SourceIndex; // Индекс начала
	//struct Link *LinkerIndex; // Индекс связки
	//struct Link *TargetIndex; // Индекс конца
	
	// ссылка на ссылающихся
	uint64_t BySource;
	uint64_t ByLinker;
	uint64_t ByTarget;
	//struct Link *FirstRefererBySource; // Ссылка на вершину дерева связей ссылающихся на эту связь в качестве начальной связи
	//struct Link *FirstRefererByLinker; // Ссылка на вершину дерева связей ссылающихся на эту связь в качестве связи связки
	//struct Link *FirstRefererByTarget; // Ссылка на вершину дерева связей ссылающихся на эту связь в качестве конечной связи
	
	// структура дерева ссылающихся
	uint64_t LeftBySource;
	uint64_t LeftByLinker;
	uint64_t LeftByTarget;
	//struct Link *NextSiblingRefererBySource; // Ссылка на правое поддерво связей ссылающихся на эту связь в качестве начальной связи
	//struct Link *NextSiblingRefererByLinker; // Ссылка на правое поддерво связей ссылающихся на эту связь в качестве связи связки
	//struct Link *NextSiblingRefererByTarget; // Ссылка на правое поддерво связей ссылающихся на эту связь в качестве конечной связи
	
	uint64_t RightBySource;
	uint64_t RightByLinker;
	uint64_t RightByTarget;
	//struct Link *PreviousSiblingRefererBySource; // Ссылка на левое поддерво связей ссылающихся на эту связь в качестве начальной связи
	//struct Link *PreviousSiblingRefererByLinker; // Ссылка на левое поддерво связей ссылающихся на эту связь в качестве связи связки
	//struct Link *PreviousSiblingRefererByTarget; // Ссылка на левое поддерво связей ссылающихся на эту связь в качестве конечной связи
	
	uint64_t BySourceCount;
	uint64_t ByLinkerCount;
	uint64_t ByTargetCount;
	//uint64_t ReferersBySourceCount; // Количество связей ссылающихся на эту связь в качестве начальной связи (количество элементов в дереве)
	//uint64_t ReferersByLinkerCount; // Количество связей ссылающихся на эту связь в качестве связи связки (количество элементов в дереве)
	//uint64_t ReferersByTargetCount; // Количество связей ссылающихся на эту связь в качестве конечной связи (количество элементов в дереве)
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
#define PREFIX_DLL __stdcall __declspec(dllexport)
#else
#define PREFIX_DLL __stdcall __declspec(dllimport)
#endif
// Linux,Unix
#else
#define PREFIX_DLL 
#endif

uint64_t PREFIX_DLL CreateLink(uint64_t sourceIndex, uint64_t linkerIndex, uint64_t targetIndex);
//Link* PREFIX_DLL CreateLink(Link* source, Link* linker, Link* target);

uint64_t PREFIX_DLL UpdateLink(uint64_t linkIndex, uint64_t sourceIndex, uint64_t linkerIndex, uint64_t targetIndex);
//Link* PREFIX_DLL UpdateLink(Link* link, Link* source, Link* linker, Link* target);

void  PREFIX_DLL DeleteLink(uint64_t linkIndex);
// void  PREFIX_DLL DeleteLink(Link* link);

uint64_t PREFIX_DLL ReplaceLink(uint64_t linkIndex, uint64_t replacementIndex);
//Link* PREFIX_DLL ReplaceLink(Link* link, Link* replacement);

//uint64_t PREFIX_DLL SearchLink(uint64_t source, uint64_t linker, uint64_t target);
uint64_t PREFIX_DLL SearchLink(uint64_t sourceIndex, uint64_t linkerIndex, uint64_t targetIndex);
//Link* PREFIX_DLL SearchLink(Link* source, Link* linker, Link* target);

uint64_t PREFIX_DLL GetLinkNumberOfReferersBySource(uint64_t linkIndex);
uint64_t PREFIX_DLL GetLinkNumberOfReferersByLinker(uint64_t linkIndex);
uint64_t PREFIX_DLL GetLinkNumberOfReferersByTarget(uint64_t linkIndex);
//uint64_t PREFIX_DLL GetLinkNumberOfReferersBySource(Link *link);
//uint64_t PREFIX_DLL GetLinkNumberOfReferersByLinker(Link *link);
//uint64_t PREFIX_DLL GetLinkNumberOfReferersByTarget(Link *link);

void PREFIX_DLL WalkThroughAllReferersBySource(uint64_t rootLinkIndex, action);
int PREFIX_DLL WalkThroughReferersBySource(uint64_t rootLinkIndex, func);
//void PREFIX_DLL WalkThroughAllReferersBySource(Link* root, action);
//int PREFIX_DLL WalkThroughReferersBySource(Link* root, func);

void PREFIX_DLL WalkThroughAllReferersByLinker(uint64_t rootLinkIndex, action);
int PREFIX_DLL WalkThroughReferersByLinker(uint64_t rootLinkIndex, func);
//void PREFIX_DLL WalkThroughAllReferersByLinker(Link* root, action);
//int PREFIX_DLL WalkThroughReferersByLinker(Link* root, func);

void PREFIX_DLL WalkThroughAllReferersByTarget(uint64_t rootLinkIndex, action);
int PREFIX_DLL WalkThroughReferersByTarget(uint64_t rootLinkIndex, func);
//void PREFIX_DLL WalkThroughAllReferersByTarget(Link* root, action);
//int PREFIX_DLL WalkThroughReferersByTarget(Link* root, func);

// not exported !!!

void AttachLinkToUnusedMarker(uint64_t linkIndex);
void DetachLinkFromUnusedMarker(uint64_t linkIndex);
//void AttachLinkToMarker(Link *link, Link *marker);
//void DetachLinkFromMarker(Link* link, Link* marker);

void DetachLink(uint64_t linkIndex);
//void DetachLink(Link* link);

#if defined(__cplusplus)
}
#endif

#endif
