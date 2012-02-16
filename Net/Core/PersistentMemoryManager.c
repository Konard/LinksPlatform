#include <Windows.h>
#include <stdio.h>

#include "Common.h"
#include "Link.h"
#include "PersistentMemoryManager.h"

int					currentMemoryPageSize;
int					mappingTableSizeInBytes;
int					serviceBlockSizeInBytes;

// Константы, расчитываемые при запуске приложения
unsigned long long	baseLinksTableBlockSizeInBytes; // Базовый размер блока данных (является минимальным размером файла, а также шагом при росте этого файла)
void*				basePersistentMemoryAddress;
void**				previousBasePersistentMemoryAddress;
unsigned long*		previousMemoryPageSize;
int*				mappingTableMaxSizeAddress;
Link**				mappingTableDataAddress;
long long*			linksTableMaxSizeAddress;
long long*			linksTableSizeAddress;
/* Не используемый блок памяти, с размером (sizeof(Link) - 16) */
Link*				linksTableUnusedLinkMarker;
Link*				linksTableDataAddress;


unsigned long long	storageFileMinSizeInBytes;

HANDLE				storageFileHandle;
HANDLE				storageFileMappingHandle;
unsigned long long	storageFileSizeInBytes;

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

int _tmain()
{
	LPVOID baseAddr;
	SIZE_T ls = GetLargestFreeMemRegion(&baseAddr);
	//_tprintf(_T("\nLargest Free Region: 0x%p bytes at 0x%p\n"), ls, baseAddr);
	return 0;
}

void InitPersistentMemoryManager()
{
	__int64 baseVirtualMemoryOffsetCounter = 2360000; //600000;
	__int64 baseVirtualMemoryOffset;
	SYSTEM_INFO info;
	SIZE_T largestMemoryBlockSize;

	GetSystemInfo(&info);

	currentMemoryPageSize = info.dwPageSize;
	
	serviceBlockSizeInBytes = info.dwPageSize * 2;
	mappingTableSizeInBytes = serviceBlockSizeInBytes - 12;
	baseLinksTableBlockSizeInBytes = currentMemoryPageSize * 256 * 4 * sizeof(Link); // ~ 512 mb

	//baseVirtualMemoryOffset = currentMemoryPageSize * baseVirtualMemoryOffsetCounter;

	largestMemoryBlockSize = GetLargestFreeMemRegion(&basePersistentMemoryAddress);

	baseVirtualMemoryOffset = (__int64) basePersistentMemoryAddress;

	//basePersistentMemoryAddress = (void*)(baseVirtualMemoryOffset);
	previousBasePersistentMemoryAddress = (void**)(baseVirtualMemoryOffset);
	previousMemoryPageSize = (unsigned long*)(baseVirtualMemoryOffset + 8);
	mappingTableMaxSizeAddress = (int*)(baseVirtualMemoryOffset + 8 + 4);
	mappingTableDataAddress = (Link**)(baseVirtualMemoryOffset + 8 + 4 + 4);
	linksTableMaxSizeAddress = (long long*)(baseVirtualMemoryOffset + serviceBlockSizeInBytes);
	linksTableSizeAddress = (long long*)(baseVirtualMemoryOffset + serviceBlockSizeInBytes + 8);
	/* Далее следует неиспользуемый блок памяти, с размером (sizeof(Link) - 16) */
	linksTableUnusedLinkMarker = (Link*)(baseVirtualMemoryOffset + serviceBlockSizeInBytes + sizeof(Link));
	linksTableDataAddress = (Link*)(baseVirtualMemoryOffset + serviceBlockSizeInBytes + 2 * sizeof(Link));

	storageFileMinSizeInBytes = serviceBlockSizeInBytes + baseLinksTableBlockSizeInBytes;
}

unsigned long OpenStorageFile(char *filename)
{
	printf("Opening file...\n");

	storageFileHandle = CreateFile(filename, GENERIC_READ | GENERIC_WRITE, NULL, NULL, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
	if (storageFileHandle == null)
	{
		unsigned long error = GetLastError();
		printf("File opening failed. Error code: %lu.\n", error);
		return error;
	}

	*((LPDWORD)&storageFileSizeInBytes) = GetFileSize(storageFileHandle, (LPDWORD)&storageFileSizeInBytes + 1);

	if (storageFileSizeInBytes < storageFileMinSizeInBytes)
		storageFileSizeInBytes = storageFileMinSizeInBytes;

	if (((storageFileSizeInBytes - serviceBlockSizeInBytes) % baseLinksTableBlockSizeInBytes) > 0)
		storageFileSizeInBytes = ((storageFileSizeInBytes - serviceBlockSizeInBytes) / baseLinksTableBlockSizeInBytes * baseLinksTableBlockSizeInBytes) + baseLinksTableBlockSizeInBytes;

	printf("File opened.\n\n");

	return 0;
}

unsigned long CloseStorageFile()
{
	printf("Closing storage file...\n");

	// При освобождении лишнего места, можно уменьшать размер файла, для этогоиспользуется функция SetEndOfFile(fh); 
	// По завершению работы с файлом можно устанавливать ограничение на размер реальных данных файла	SetFileValidData(fh,newFileLen); 

	if (storageFileHandle == null)
	{
		unsigned long error = -1;
		printf("Storage file is not open or already closed.\n\n");
		return error;
	}

	CloseHandle(storageFileHandle);
	storageFileHandle = null;
	storageFileSizeInBytes = 0;

	printf("Storage file closed.\n\n");
		
	return 0;
}

unsigned long EnlargeStorageFile()
{
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

	return 0;
}

unsigned long ShrinkStorageFile()
{
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

	return 0;
}

unsigned long SetStorageFileMemoryMapping()
{
	printf("Setting memory mapping of storage file...\n");

	storageFileMappingHandle = CreateFileMapping(storageFileHandle, NULL, PAGE_READWRITE, 0, storageFileSizeInBytes, NULL);
	if (storageFileMappingHandle == null)
	{
		unsigned long error = GetLastError();
		printf("Mapping creation failed. Error code: %lu.\n\n", error);
		return error;
	}

	basePersistentMemoryAddress = MapViewOfFileEx(storageFileMappingHandle, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0, basePersistentMemoryAddress);
	if (basePersistentMemoryAddress == null) 
	{
		unsigned long error = GetLastError();
		printf("Mapping view set failed. Error code: %lu.\n\n", error);
		return error;
	}

	// Выполняем первоначальную инициализацию и валидацию основных вспомогательных счётчиков и значений
	if (*previousBasePersistentMemoryAddress == null)
		*previousBasePersistentMemoryAddress = basePersistentMemoryAddress;

	if (*previousMemoryPageSize == 0)
		*previousMemoryPageSize = currentMemoryPageSize;

	if (*previousBasePersistentMemoryAddress != basePersistentMemoryAddress)
	{
		unsigned long error = -1;
		printf("Saved links table located in different address offset. Previous address is %I64d instead of %I64d.\n\n", *previousBasePersistentMemoryAddress, basePersistentMemoryAddress);
		return error;
	}

	if (*previousMemoryPageSize != currentMemoryPageSize)
	{
		unsigned long error = -2;
		printf("Saved links table was mapped with different memory page size. Memory page size is %lu instead of %lu.\n\n", *previousMemoryPageSize, currentMemoryPageSize);
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
		unsigned long error = -3;
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

	printf("Memory mapping of storage file is reset.\n\n");

	return 0;
}

void PrintLinksTableSize()
{
	printf("Links table size: %I64d links, %I64d bytes.\n", *linksTableSizeAddress, *linksTableSizeAddress * sizeof(Link));
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

void WalkThroughAllLinks(void __stdcall func(Link *))
{
	Link *currentLink = linksTableDataAddress;
	Link* lastLink = linksTableDataAddress + *linksTableSizeAddress - 1;

	do
	{
		if (currentLink->Linker != linksTableUnusedLinkMarker)
		{
			func(currentLink);
		}
	}
	while(++currentLink <= lastLink);
}

int WalkThroughLinks(int __stdcall func(Link *))
{
	Link *currentLink = linksTableDataAddress;
	Link* lastLink = linksTableDataAddress + *linksTableSizeAddress - 1;

	do
	{
		if (currentLink->Linker != linksTableUnusedLinkMarker)
		{
			if(!func(currentLink)) return false;
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
	__int64 resultCounter = 0;

	printf("Reading data...\n");

	{
		__int64* intMap = (__int64*) basePersistentMemoryAddress;
		__int64* intMapLastAddress = intMap + (storageFileSizeInBytes / sizeof(__int64) - 1);
		__int64* intCurrent = intMap;

		for(; intCurrent <= intMapLastAddress; intCurrent++)
		{
			resultCounter += *intCurrent;
		}
	}

	printf("Data read. Counter result is: %I64d.\n\n", resultCounter);
}

void WriteTest()
{
	printf("Filling data...\n");

	{
		__int64* intMap = (__int64*) basePersistentMemoryAddress;
		__int64* intMapLastAddress = intMap + (storageFileSizeInBytes / sizeof(__int64) - 1);
		__int64* intCurrent = intMap;

		for(; intCurrent <= intMapLastAddress; intCurrent++)
		{
			*intCurrent = intCurrent - intMap;
		}
	}

	printf("Data filled.\n\n");
}
