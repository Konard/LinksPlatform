#ifndef __LINKS_PERSISTENT_MEMORY_MANAGER_H__
#define __LINKS_PERSISTENT_MEMORY_MANAGER_H__

// Менеджер памяти (memory manager).

#include "Common.h"
#include "Link.h"

#if defined(__cplusplus)
extern "C" {
#endif

// see http://stackoverflow.com/questions/538134/exporting-functions-from-a-dll-with-dllexport

#if defined(_MSC_VER) || defined(__MINGW32__) || defined(__MINGW64__)
#if defined(LINKS_DLL_EXPORT)
#define PREFIX_DLL __declspec(dllexport)
#else
#define PREFIX_DLL __declspec(dllimport)
#endif
#elif defined(__GLIBC__)
// for Linux,Unix?
#define PREFIX_DLL 
#endif

PREFIX_DLL void InitPersistentMemoryManager();
PREFIX_DLL int OpenStorageFile(char *filename);
PREFIX_DLL int CloseStorageFile();
PREFIX_DLL unsigned long EnlargeStorageFile();
PREFIX_DLL unsigned long ShrinkStorageFile();
PREFIX_DLL int SetStorageFileMemoryMapping();
PREFIX_DLL unsigned long ResetStorageFileMemoryMapping();
PREFIX_DLL uint64_t GetMappedLink(int index);
PREFIX_DLL void SetMappedLink(int index, uint64_t linkIndex);
PREFIX_DLL void ReadTest();
PREFIX_DLL void WriteTest();

uint64_t AllocateLink();
void FreeLink(uint64_t linkIndex);

PREFIX_DLL void WalkThroughAllLinks(func);
PREFIX_DLL int WalkThroughLinks(func);

Link *GetLink(uint64_t linkIndex);

#if defined(__cplusplus)
}
#endif

#endif
