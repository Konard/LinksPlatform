#if defined(_MFC_VER) || defined(__MINGW32__)
#include <windows.h>
#elif defined(__GNUC__)
#include <unistd.h>
// open()
#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
// errno
#include <errno.h>
// mmap()...
#include <sys/mman.h>
#endif

#include <stdio.h>


#include "Common.h"
#include "Link.h"
#include "PersistentMemoryManager.h"

int					currentMemoryPageSize;
int					mappingTableSizeInBytes;
int					serviceBlockSizeInBytes;

// Константы, расчитываемые при запуске приложения
uint64_t			baseLinksTableBlockSizeInBytes; // Базовый размер блока данных (является минимальным размером файла, а также шагом при росте этого файла)
void*				basePersistentMemoryAddress; // result of mmap()
void**				previousBasePersistentMemoryAddress;
int*				previousMemoryPageSize;
int*				mappingTableMaxSizeAddress;
Link**				mappingTableDataAddress;
uint64_t*			linksTableMaxSizeAddress;
uint64_t*			linksTableSizeAddress;
/* Не используемый блок памяти, с размером (sizeof(Link) - 16) */
Link*				linksTableUnusedLinkMarker;
Link*				linksTableDataAddress; // здесь хранятся линки


uint64_t			storageFileMinSizeInBytes;

// Дескриптор файла базы данных и дескриптор объекта отображения (map)
#if defined(_MFC_VER) || defined(__MINGW32__)
HANDLE				storageFileHandle;
HANDLE				storageFileMappingHandle;
#elif defined(__GNUC__)
int				storageFileHandle; // для open()
//int				storageFileMappingHandle;
#endif
uint64_t				storageFileSizeInBytes; // <- off_t

void PrintLinksTableSize()
{
#if defined(_MFC_VER) || defined(__MINGW32__)
	printf("Links table size: %I64d links, %I64d bytes.\n", *linksTableSizeAddress, *linksTableSizeAddress * sizeof(Link));
#elif defined(__GNUC__)
#endif
}

#if defined(_MFC_VER) || defined(__MINGW32__)
SIZE_T GetLargestFreeMemRegion(LPVOID *lpBaseAddr)
{
	SYSTEM_INFO systemInfo;
	MEMORY_BASIC_INFORMATION mbi;
	VOID *p = 0;
	SIZE_T largestSize = 0;

	GetSystemInfo(&systemInfo);
	//p = 0;
	//largestSize = 0;

	while(p < systemInfo.lpMaximumApplicationAddress)
	{
		SIZE_T dwRet = VirtualQuery(p, &mbi, sizeof(mbi));
		if (dwRet > 0)
		{
			if (mbi.State == MEM_FREE)
			{
				if (largestSize < mbi.RegionSize)
				{
					largestSize = mbi.RegionSize;
					if (lpBaseAddr != NULL)
					*lpBaseAddr = mbi.BaseAddress;
				}
			}
			p = (void*) (((char*)p) + mbi.RegionSize);
		}
		else
		{
			p = (void*) (((char*)p) + systemInfo.dwPageSize);
		}
	}
	return largestSize;
}
#elif defined(__GNUC__)
uint64_t GetLargestFreeMemRegion()
{
    return 0;
}
#endif

/*
int _tmain()
{
	LPVOID baseAddr;
	SIZE_T ls = GetLargestFreeMemRegion(&baseAddr);
	//_tprintf(_T("\nLargest Free Region: 0x%p bytes at 0x%p\n"), ls, baseAddr);
	return 0;
}
*/

void InitPersistentMemoryManager()
{
//	uint64_t baseVirtualMemoryOffsetCounter = 2360000; //600000; //? Почему именно такой отступ?
	uint64_t baseVirtualMemoryOffset;

#if defined(_MFC_VER) || defined(__MINGW32__)

	SYSTEM_INFO info; // см. http://msdn.microsoft.com/en-us/library/windows/desktop/ms724958%28v=vs.85%29.aspx
	/* 
typedef struct _SYSTEM_INFO {
  union {
    DWORD  dwOemId;
    struct {
      WORD wProcessorArchitecture;
      WORD wReserved;
    };
  };
  DWORD     dwPageSize;
  LPVOID    lpMinimumApplicationAddress;
  LPVOID    lpMaximumApplicationAddress;
  DWORD_PTR dwActiveProcessorMask;
  DWORD     dwNumberOfProcessors;
  DWORD     dwProcessorType;
  DWORD     dwAllocationGranularity;
  WORD      wProcessorLevel;
  WORD      wProcessorRevision;
} SYSTEM_INFO;

	*/
	SIZE_T largestMemoryBlockSize;

	GetSystemInfo(&info);

	currentMemoryPageSize = info.dwPageSize;
	serviceBlockSizeInBytes = info.dwPageSize * 2;

	// см. 
	largestMemoryBlockSize = GetLargestFreeMemRegion(&basePersistentMemoryAddress);

#elif defined(__GNUC__)

	long sz = sysconf(_SC_PAGESIZE);
	currentMemoryPageSize = sz; // ? привести к одному типу
	serviceBlockSizeInBytes = sz * 2;
	// отсутствует largestMemoryBlockSize = ...

#endif

	mappingTableSizeInBytes = serviceBlockSizeInBytes - 12;
	baseLinksTableBlockSizeInBytes = currentMemoryPageSize * 256 * 4 * sizeof(Link); // ~ 512 mb

	//baseVirtualMemoryOffset = currentMemoryPageSize * baseVirtualMemoryOffsetCounter;

	baseVirtualMemoryOffset = (uint64_t) basePersistentMemoryAddress;

	//basePersistentMemoryAddress = (void*)(baseVirtualMemoryOffset);
	previousBasePersistentMemoryAddress = (void**)(baseVirtualMemoryOffset);
	previousMemoryPageSize = (int *)(baseVirtualMemoryOffset + 8);
	mappingTableMaxSizeAddress = (int*)(baseVirtualMemoryOffset + 8 + 4);
	mappingTableDataAddress = (Link**)(baseVirtualMemoryOffset + 8 + 4 + 4);
	linksTableMaxSizeAddress = (uint64_t *)(baseVirtualMemoryOffset + serviceBlockSizeInBytes);
	linksTableSizeAddress = (uint64_t *)(baseVirtualMemoryOffset + serviceBlockSizeInBytes + 8);
	/* Далее следует неиспользуемый блок памяти, с размером (sizeof(Link) - 16) */
	linksTableUnusedLinkMarker = (Link*)(baseVirtualMemoryOffset + serviceBlockSizeInBytes + sizeof(Link));
	linksTableDataAddress = (Link*)(baseVirtualMemoryOffset + serviceBlockSizeInBytes + 2 * sizeof(Link));

	storageFileMinSizeInBytes = serviceBlockSizeInBytes + baseLinksTableBlockSizeInBytes;
	return;
}

// DWORD == int
int OpenStorageFile(char *filename)
{
	printf("Opening file...\n");

#if defined(_MFC_VER) || defined(__MINGW32__)
	// см. MSDN "CreateFile function", http://msdn.microsoft.com/en-us/library/windows/desktop/aa363858%28v=vs.85%29.aspx
	storageFileHandle = CreateFile(filename, GENERIC_READ | GENERIC_WRITE, NULL, NULL, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
	if (storageFileHandle == INVALID_HANDLE_VALUE)
	{
		// см. MSDN "GetLastError function", http://msdn.microsoft.com/en-us/library/windows/desktop/ms679360%28v=vs.85%29.aspx
		int error = GetLastError();
		printf("File opening failed. Error code: %d.\n", error);
		return error;
	}
	// см. MSDN "GetFileSize function", http://msdn.microsoft.com/en-us/library/windows/desktop/aa364955%28v=vs.85%29.aspx
	*((LPDWORD)&storageFileSizeInBytes) = GetFileSize(storageFileHandle, (LPDWORD)&storageFileSizeInBytes + 1);
	if (storageFileSizeInBytes == INVALID_FILE_SIZE)
	{
		int error = GetLastError();
		printf("File size get failed. Error code: %d.\n", error);
		return error;
	}

#elif defined(__GNUC__)
	storageFileHandle = open(filename, O_CREAT, S_IRUSR | S_IWUSR);
	if (storageFileHandle == -1) {
		int error = errno;
		printf("File opening failed. Error code: %d.\n", error);
		return error;
	}
	struct stat statbuf;
	if (fstat(storageFileHandle, &statbuf) != 0)
	{
		int error = errno;
		printf("File size get failed. Error code: %d.\n", error);
		return error;
	}
	storageFileSizeInBytes = statbuf.st_size; // ? uint64_t = off_t
#endif

	if (storageFileSizeInBytes < storageFileMinSizeInBytes)
		storageFileSizeInBytes = storageFileMinSizeInBytes;

	if (((storageFileSizeInBytes - serviceBlockSizeInBytes) % baseLinksTableBlockSizeInBytes) > 0)
		storageFileSizeInBytes = ((storageFileSizeInBytes - serviceBlockSizeInBytes) / baseLinksTableBlockSizeInBytes * baseLinksTableBlockSizeInBytes) + baseLinksTableBlockSizeInBytes;

	printf("File %s opened.\n\n", filename);

	return 0;
}

int CloseStorageFile()
{
	printf("Closing storage file...\n");

	// При освобождении лишнего места, можно уменьшать размер файла, для этогоиспользуется функция SetEndOfFile(fh); 
	// По завершению работы с файлом можно устанавливать ограничение на размер реальных данных файла	SetFileValidData(fh,newFileLen); 

#if defined(_MFC_VER) || defined(__MINGW32__)
	if (storageFileHandle == null)
	{
		unsigned long error = -1;
		printf("Storage file is not open or already closed.\n\n");
		return error;
	}

	CloseHandle(storageFileHandle);
	storageFileHandle = null;
	storageFileSizeInBytes = 0;
#elif defined(__GNUC__)
	if (storageFileHandle == -1)
	{
		printf("Storage file is not open or already closed.\n\n");
		return -1;
	}

	close(storageFileHandle);
	storageFileHandle = 0;
	storageFileSizeInBytes = 0;
#endif

	printf("Storage file closed.\n\n");
		
	return 0;
}

unsigned long EnlargeStorageFile()
{
#if defined(_MFC_VER) || defined(__MINGW32__)
	if (storageFileHandle == null)
	{
		unsigned long error = -1;
		printf("Storage file is not open.\n");
		return error;
	}

	if (storageFileSizeInBytes >= storageFileMinSizeInBytes)
	{
		unsigned long error = 0;

		error = ResetStorageFileMemoryMapping();
		if(error != 0)
			return error;

		storageFileSizeInBytes += baseLinksTableBlockSizeInBytes;

		error = SetStorageFileMemoryMapping();
		if(error != 0)
			return error;
	}
#elif defined(__GNUC__)
#endif

	return 0;
}

unsigned long ShrinkStorageFile()
{
#if defined(_MFC_VER) || defined(__MINGW32__)
	if (storageFileHandle == null)
	{
		unsigned long error = -1;
		printf("Storage file is not open.\n");
		return error;
	}

	if (storageFileSizeInBytes > storageFileMinSizeInBytes)
	{
		unsigned long error = 0;
		unsigned long long linksTableNewMaxSize = (storageFileSizeInBytes - baseLinksTableBlockSizeInBytes - serviceBlockSizeInBytes - 2 * sizeof(Link)) / sizeof(Link);

		if (*linksTableSizeAddress < linksTableNewMaxSize)
		{
			error = ResetStorageFileMemoryMapping();
			if(error != 0)
				return error;

			{
				LARGE_INTEGER distanceToMoveFilePointer;
				distanceToMoveFilePointer.QuadPart = -((long long)baseLinksTableBlockSizeInBytes);

				storageFileSizeInBytes -= baseLinksTableBlockSizeInBytes;

				SetFilePointerEx(storageFileHandle, distanceToMoveFilePointer, NULL, FILE_END);
				SetEndOfFile(storageFileHandle);
			}

			error = SetStorageFileMemoryMapping();
			if(error != 0)
				return error;
		}
	}
#elif defined(__GNUC__)
#endif

	return 0;
}

// стандартный тип ошибки - int
int SetStorageFileMemoryMapping()
{
	printf("Setting memory mapping of storage file...\n");

#if defined(_MFC_VER) || defined(__MINGW32__)
	storageFileMappingHandle = CreateFileMapping(storageFileHandle, NULL, PAGE_READWRITE, 0, storageFileSizeInBytes, NULL);
	if (storageFileMappingHandle == null)
	{
		unsigned long error = GetLastError();
		printf("Mapping creation failed. Error code: %lu.\n\n", error);
		return error;
	}

	// аналог mmap(), см. http://msdn.microsoft.com/en-us/library/windows/desktop/aa366763%28v=vs.85%29.aspx
	// http://msdn.microsoft.com/en-us/library/windows/desktop/aa366761%28v=vs.85%29.aspx
	// hFileMappingObject [in] A handle to a file mapping object. The CreateFileMapping and OpenFileMapping functions return this handle.
	basePersistentMemoryAddress = MapViewOfFileEx(storageFileMappingHandle, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0, basePersistentMemoryAddress);
	if (basePersistentMemoryAddress == null) 
	{
		unsigned long error = GetLastError();
		printf("Mapping view set failed. Error code: %lu.\n\n", error);
		return error;
	}
#elif defined(__GNUC__)
	basePersistentMemoryAddress = mmap(NULL, storageFileSizeInBytes, PROT_READ | PROT_WRITE, MAP_SHARED, storageFileHandle, 0);
	if (basePersistentMemoryAddress == MAP_FAILED)
	{
		int error = errno;
		printf("Mapping view set failed. Error code: %d.\n\n", error);
		return error;
	}
#endif

	// Выполняем первоначальную инициализацию и валидацию основных вспомогательных счётчиков и значений
	if (*previousBasePersistentMemoryAddress == null)
		*previousBasePersistentMemoryAddress = basePersistentMemoryAddress;

	if (*previousMemoryPageSize == 0)
		*previousMemoryPageSize = currentMemoryPageSize;

	if (*previousBasePersistentMemoryAddress != basePersistentMemoryAddress)
	{
		int error = -1;
		printf("Saved links table located in different address offset. Previous address is %llu instead of %llu.\n\n",
		    (unsigned long long)*previousBasePersistentMemoryAddress,
		    (unsigned long long)basePersistentMemoryAddress);
		return error;
	}

	if (*previousMemoryPageSize != currentMemoryPageSize)
	{
		int error = -2;
		printf("Saved links table was mapped with different memory page size. Memory page size is %llu instead of %llu.\n\n",
		    (unsigned long long)*previousMemoryPageSize,
		    (unsigned long long)currentMemoryPageSize);
		return error;
	}

	if (*mappingTableMaxSizeAddress == 0)
		*mappingTableMaxSizeAddress = (mappingTableSizeInBytes - 4) / 8;

	if (*linksTableMaxSizeAddress == 0)
		*linksTableMaxSizeAddress = (storageFileSizeInBytes - serviceBlockSizeInBytes - 2 * sizeof(Link)) / sizeof(Link);
	else if (*linksTableMaxSizeAddress != (storageFileSizeInBytes - serviceBlockSizeInBytes - 2 * sizeof(Link)) / sizeof(Link))
		*linksTableMaxSizeAddress = (storageFileSizeInBytes - serviceBlockSizeInBytes - 2 * sizeof(Link)) / sizeof(Link);

	if (*linksTableSizeAddress > *linksTableMaxSizeAddress)
	{
		int error = -3;
		printf("Saved links table size counter is set to bigger value than maximum allowed table size.\n\n");
		return error;
	}

	printf("Memory mapping of storage file is set.\n");

	PrintLinksTableSize();

	printf("\n");

	return 0;
}

unsigned long ResetStorageFileMemoryMapping()
{
	printf("Resetting memory mapping of storage file...\n");

#if defined(_MFC_VER) || defined(__MINGW32__)
	if (storageFileMappingHandle == null)
	{
		unsigned long error = -1;
		printf("Memory mapping of storage file is not set or already reset.\n");
		return error;
	}

	PrintLinksTableSize();

	UnmapViewOfFile (basePersistentMemoryAddress);
	CloseHandle(storageFileMappingHandle);
	storageFileMappingHandle = null;
#elif defined(__GNUC__)
#endif

	printf("Memory mapping of storage file is reset.\n\n");

	return 0;
}

Link* AllocateFromUnusedLinks()
{
	Link* unusedLink = linksTableUnusedLinkMarker->FirstRefererByLinker;
	DetachLinkFromMarker(unusedLink, linksTableUnusedLinkMarker);
	return unusedLink;
}

Link* AllocateFromFreeLinks()
{
	Link* freeLink;

	if (*linksTableMaxSizeAddress == *linksTableSizeAddress)
		EnlargeStorageFile();

	freeLink = linksTableDataAddress + *linksTableSizeAddress;
	++*linksTableSizeAddress;
	return freeLink;
}

Link* AllocateLink()
{
	if (linksTableUnusedLinkMarker->FirstRefererByLinker != null)
		return AllocateFromUnusedLinks();
	else
		return AllocateFromFreeLinks();	
	return null;
}

void FreeLink(Link* link)
{
	DetachLink(link);

    while (link->FirstRefererBySource != null) FreeLink(link->FirstRefererBySource);
    while (link->FirstRefererByLinker != null) FreeLink(link->FirstRefererByLinker);
    while (link->FirstRefererByTarget != null) FreeLink(link->FirstRefererByTarget);

	{
		Link* lastUsedLink = linksTableDataAddress + *linksTableSizeAddress - 1;

		if (link < lastUsedLink)
		{
			AttachLinkToMarker(link, linksTableUnusedLinkMarker);
		}
		else if(link == lastUsedLink)
		{
			--*linksTableSizeAddress;

			while((--lastUsedLink)->Linker == linksTableUnusedLinkMarker)
			{
				DetachLinkFromMarker(lastUsedLink, linksTableUnusedLinkMarker);
				--*linksTableSizeAddress;
			}

			ShrinkStorageFile(); // Размер будет уменьшен, только если допустимо
		}
	}
}

void WalkThroughAllLinks(func func_)
{
	Link *currentLink = linksTableDataAddress;
	Link* lastLink = linksTableDataAddress + *linksTableSizeAddress - 1;

	do
	{
		if (currentLink->Linker != linksTableUnusedLinkMarker)
		{
			func_(currentLink);
		}
	}
	while(++currentLink <= lastLink);
}

int WalkThroughLinks(func func_)
{
	Link *currentLink = linksTableDataAddress;
	Link* lastLink = linksTableDataAddress + *linksTableSizeAddress - 1;

	do
	{
		if (currentLink->Linker != linksTableUnusedLinkMarker)
		{
			if(!func_(currentLink)) return false;
		}
	}
	while(++currentLink <= lastLink);
	
	return true;
}

Link* GetMappedLink(int index)
{
	if (index < *mappingTableMaxSizeAddress)
		return mappingTableDataAddress[index];
	else
		return null;
}

void SetMappedLink(int index, Link* link)
{
	if (index < *mappingTableMaxSizeAddress)
		mappingTableDataAddress[index] = link;
}

void ReadTest()
{

	printf("Reading data...\n");

	uint64_t resultCounter = 0;

#if defined(_MFC_VER) || defined(__MINGW32__)
	{
		uint64_t* intMap = (uint64_t *) basePersistentMemoryAddress;
		uint64_t* intMapLastAddress = intMap + (storageFileSizeInBytes / sizeof(__int64) - 1);
		uint64_t* intCurrent = intMap;

		for(; intCurrent <= intMapLastAddress; intCurrent++)
		{
			resultCounter += *intCurrent;
		}
	}
	printf("Data read. Counter result is: %I64d.\n\n", resultCounter);
#elif defined(__GNUC__)
	printf("Data read. Counter result is: %lld.\n\n", (long long int)resultCounter);
#endif

}

void WriteTest()
{
	printf("Filling data...\n");

#if defined(_MFC_VER) || defined(__MINGW32__)
	{
		uint64_t* intMap = (uint64_t *) basePersistentMemoryAddress;
		uint64_t* intMapLastAddress = intMap + (storageFileSizeInBytes / sizeof(__int64) - 1);
		uint64_t* intCurrent = intMap;

		for(; intCurrent <= intMapLastAddress; intCurrent++)
		{
			*intCurrent = intCurrent - intMap;
		}
	}
#elif defined(__GNUC__)
#endif

	printf("Data filled.\n\n");
}
