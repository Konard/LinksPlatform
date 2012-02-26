#ifndef __LINKS_PERSISTENT_MEMORY_MANAGER_H__
#define __LINKS_PERSISTENT_MEMORY_MANAGER_H__

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


#if defined(_MFC_VER)
#elif defined(__GNUC__)
#endif

PREFIX_DLL void InitPersistentMemoryManager();
PREFIX_DLL int OpenStorageFile(char *filename);
PREFIX_DLL int CloseStorageFile();
PREFIX_DLL unsigned long EnlargeStorageFile();
PREFIX_DLL unsigned long ShrinkStorageFile();
PREFIX_DLL int SetStorageFileMemoryMapping();
PREFIX_DLL unsigned long ResetStorageFileMemoryMapping();

PREFIX_DLL uint64_t GetBaseLink(uint32_t index);
PREFIX_DLL void SetBaseLink(uint32_t index, uint64_t linkIndex);
//PREFIX_DLL Link* GetMappedLink(int index);
//PREFIX_DLL void SetMappedLink(int index, Link* link);

PREFIX_DLL void ReadTest();
PREFIX_DLL void WriteTest();

uint64_t AllocateLink();
void FreeLink(uint64_t);
//Link* AllocateLink();
//void FreeLink(Link* link);

PREFIX_DLL void WalkThroughAllLinks(func);
PREFIX_DLL int WalkThroughLinks(func);

PREFIX_DLL Link* GetLink(uint64_t linkIndex); // из таблицы pointerToLinks

PREFIX_DLL uint64_t GetSourceIndex(uint64_t linkIndex);
PREFIX_DLL uint64_t GetTargetIndex(uint64_t linkIndex);
PREFIX_DLL uint64_t GetLinkerIndex(uint64_t linkIndex); // LinkerLink index
PREFIX_DLL uint64_t GetBySourceIndex(uint64_t linkIndex);
PREFIX_DLL uint64_t GetByTargetIndex(uint64_t linkIndex);
PREFIX_DLL uint64_t GetByLinkerIndex(uint64_t linkIndex);


#if defined(__cplusplus)
}
#endif

#endif
