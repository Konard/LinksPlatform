#ifndef __LINKS_PERSISTENT_MEMORY_MANAGER_H__
#define __LINKS_PERSISTENT_MEMORY_MANAGER_H__

// Persistent on drive memory manager (Менеджер хранимой на диске памяти).

#include "Common.h"
#include "Link.h"

#if defined(__cplusplus)
extern "C" {
#endif

    PREFIX_DLL void InitPersistentMemoryManager();
    PREFIX_DLL int OpenStorageFile(char *filename);
    PREFIX_DLL int CloseStorageFile();
    PREFIX_DLL int EnlargeStorageFile();
    PREFIX_DLL int ShrinkStorageFile();
    PREFIX_DLL int SetStorageFileMemoryMapping();
    PREFIX_DLL int ResetStorageFileMemoryMapping();
    PREFIX_DLL link_index GetMappedLink(int mappedIndex);
    PREFIX_DLL void SetMappedLink(int mappedIndex, link_index linkIndex);
    PREFIX_DLL void ReadTest();
    PREFIX_DLL void WriteTest();

    link_index AllocateLink();
    void FreeLink(link_index link);

    PREFIX_DLL void WalkThroughAllLinks(visitor visitor);
    PREFIX_DLL signed_integer WalkThroughLinks(stoppable_visitor stoppableVisitor);

    __forceinline Link *GetLink(link_index linkIndex);

#if defined(__cplusplus)
}
#endif

#endif
