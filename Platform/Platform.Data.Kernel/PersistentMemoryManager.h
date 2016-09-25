#ifndef __LINKS_PERSISTENT_MEMORY_MANAGER_H__
#define __LINKS_PERSISTENT_MEMORY_MANAGER_H__

// Persistent on drive memory manager (Менеджер хранимой на диске памяти).

#include "Common.h"
#include "Link.h"

#if defined(__cplusplus)
extern "C" {
#endif

    void InitPersistentMemoryManager();
    
    signed_integer OpenStorageFile(char* filename);
    signed_integer CloseStorageFile();
    signed_integer EnlargeStorageFile();
    signed_integer ShrinkStorageFile();
    signed_integer SetStorageFileMemoryMapping();
    signed_integer ResetStorageFileMemoryMapping();
    
    PREFIX_DLL signed_integer OpenLinks(char* filename);
    PREFIX_DLL signed_integer CloseLinks();
    
    PREFIX_DLL link_index GetMappedLink(signed_integer mappedIndex);
    PREFIX_DLL void SetMappedLink(signed_integer mappedIndex, link_index linkIndex);

    PREFIX_DLL void WalkThroughAllLinks(visitor visitor);
    PREFIX_DLL signed_integer WalkThroughLinks(stoppable_visitor stoppableVisitor);

    PREFIX_DLL unsigned_integer GetLinksCount();

    // Exported only for Tests, Unsafe to use directly (use Create/Update/Delete instead)
    PREFIX_DLL link_index AllocateLink();
    PREFIX_DLL void FreeLink(link_index link);

    Link* GetLink(link_index linkIndex);
    link_index GetLinkIndex(Link* link);

#if defined(__cplusplus)
}
#endif

#define LINKS_DATA_SEAL_64BIT 0x810118808100180
// Binary:
// 0000100000010000
// 0001000110001000
// 0000100000010000
// 0000000110000000‬
#endif
