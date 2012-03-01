#ifndef __LINKS_LINK_H__
#define __LINKS_LINK_H__

// Всё, что достаточно для работы с линками + ??? Common.h (?)

#include "Common.h"

typedef struct Link
{
	// заменяем всё на индексы
	uint64_t Source; // Индекс начала
	uint64_t Linker; // Индекс связки
	uint64_t Target; // Индекс конца
	
	// ссылка на ссылающихся
	uint64_t BySource; // Индекс вершины дерева (связей, использующих данную в качестве Source)
	uint64_t ByLinker; // Индекс вершины дерева (связей, использующих данную в качестве Linker)
	uint64_t ByTarget; // Индекс вершины дерева (связей, использующих данную в качестве Target)
	
	// структура дерева ссылающихся
	uint64_t LeftBySource; // левое поддерево связей (использующих данную в качестве Source)
	uint64_t LeftByLinker; // левое поддерево связей (использующих данную в качестве Linker)
	uint64_t LeftByTarget; // левое поддерево связей (использующих данную в качестве Target)
	
	uint64_t RightBySource; // правое поддерево связей (использующих данную в качестве Source)
	uint64_t RightByLinker; // правое поддерево связей (использующих данную в качестве Linker)
	uint64_t RightByTarget; // правое поддерево связей (использующих данную в качестве Target)
	
	uint64_t CountBySource; // Число связей, использующих данную в качестве Source
	uint64_t CountByLinker; // Число связей, использующих данную в качестве Linker
	uint64_t CountByTarget; // Число связей, использующих данную в качестве Target
	
	int64_t Timestamp; // используется :)
} Link;

typedef int (*func)(uint64_t); // callback
typedef void (*action)(uint64_t); // callback


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

// Созвращает индекс линка
uint64_t PREFIX_DLL CreateLink(uint64_t sourceIndex, uint64_t linkerIndex, uint64_t targetIndex);
uint64_t PREFIX_DLL UpdateLink(uint64_t linkIndex, uint64_t sourceIndex, uint64_t linkerIndex, uint64_t targetIndex);

void  PREFIX_DLL DeleteLink(uint64_t linkIndex);

// ??? Аналог ассемблерного оператора MOV (в языка высокого уровня - оператор приравнивания)
uint64_t PREFIX_DLL ReplaceLink(uint64_t linkIndex, uint64_t replacementIndex);

uint64_t PREFIX_DLL SearchLink(uint64_t sourceIndex, uint64_t linkerIndex, uint64_t targetIndex);

// Возвращает число By-Count
uint64_t PREFIX_DLL GetLinkNumberOfReferersBySource(uint64_t linkIndex);
uint64_t PREFIX_DLL GetLinkNumberOfReferersByLinker(uint64_t linkIndex);
uint64_t PREFIX_DLL GetLinkNumberOfReferersByTarget(uint64_t linkIndex);

// ??? Обход
void PREFIX_DLL WalkThroughAllReferersBySource(uint64_t rootLinkIndex, action);
int PREFIX_DLL WalkThroughReferersBySource(uint64_t rootLinkIndex, func);

void PREFIX_DLL WalkThroughAllReferersByLinker(uint64_t rootLinkIndex, action);
int PREFIX_DLL WalkThroughReferersByLinker(uint64_t rootLinkIndex, func);

void PREFIX_DLL WalkThroughAllReferersByTarget(uint64_t rootLinkIndex, action);
int PREFIX_DLL WalkThroughReferersByTarget(uint64_t rootLinkIndex, func);

// ??? not for export !!!

void AttachLinkToUnusedMarker(uint64_t linkIndex);
void DetachLinkFromUnusedMarker(uint64_t linkIndex);

void DetachLink(uint64_t linkIndex);

#if defined(__cplusplus)
}
#endif

#endif
