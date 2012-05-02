#if defined(_MSC_VER) || defined(__MINGW32__)
#include <windows.h>
#elif defined(__GNUC__)

// for 64-bit files
#define _XOPEN_SOURCE 700
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

// Дескриптор файла базы данных и дескриптор объекта отображения (map)
#if defined(_MSC_VER) || defined(__MINGW32__)
HANDLE			storageFileHandle;
HANDLE			storageFileMappingHandle;
#elif defined(__GNUC__)
int			storageFileHandle; // для open()
#endif
uint64_t		storageFileSizeInBytes; // <- off_t


// Константы, рассчитываемые при запуске приложения
int			currentMemoryPageSizeInBytes;	// Размер страницы в операционной системе
int			serviceBlockSizeInBytes;	// Размер сервисных данных, (две страницы)
int			baseLinksSizeInBytes;	
uint64_t		baseBlockSizeInBytes; // Базовый размер блока данных (является минимальным размером файла, а также шагом при росте этого файла)
uint64_t		storageFileMinSizeInBytes;

void*			pointerToMappedRegion; 		// указатель на начало региона памяти - результата mmap()

int*				pointerToBaseLinksMaxSize;
Link**				pointerToPointerToBaseLinks;	// инициализируется в SetStorageFileMemoryMapping()
uint64_t*			pointerToLinksMaxSize;
uint64_t*			pointerToLinksSize;

/* Не используемый блок памяти, с размером (sizeof(Link) - 16) */
Link*				pointerToUnusedMarker;	// инициализируется в SetStorageFileMemoryMapping()
Link*				pointerToLinks;		// здесь хранятся линки, инициализируется в SetStorageFileMemoryMapping()



/***  Работа с памятью  ***/

void PrintLinksTableSize()
{
#if defined(_MSC_VER) || defined(__MINGW32__)
	printf("Links table size: %I64d links, %I64d bytes.\n", *pointerToLinksSize, *pointerToLinksSize * sizeof(Link));
#elif defined(__GNUC__)
	printf("Links table size: %llu links, %llu bytes.\n",
	  (long long unsigned int)(*pointerToLinksSize),
	  (long long unsigned int)(*pointerToLinksSize * sizeof(Link)));
#endif
}

void InitPersistentMemoryManager()
{

#if defined(_MSC_VER) || defined(__MINGW32__)

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

	SYSTEM_INFO info; // см. http://msdn.microsoft.com/en-us/library/windows/desktop/ms724958%28v=vs.85%29.aspx
	GetSystemInfo(&info);
	currentMemoryPageSizeInBytes = info.dwPageSize;
	serviceBlockSizeInBytes = info.dwPageSize * 2;

#elif defined(__GNUC__)

	long sz = sysconf(_SC_PAGESIZE);
	currentMemoryPageSizeInBytes = sz; // ? привести к одному типу
	serviceBlockSizeInBytes = sz * 2;
	printf("_SC_PAGESIZE = %lu\n", sz);

#endif

	baseLinksSizeInBytes = serviceBlockSizeInBytes - 12;
	baseBlockSizeInBytes = currentMemoryPageSizeInBytes * 256 * 4 * sizeof(Link); // ~ 512 mb

	storageFileMinSizeInBytes = serviceBlockSizeInBytes + baseBlockSizeInBytes;

	printf("storageFileMinSizeInBytes = %llu\n",
	       (long long unsigned int)storageFileMinSizeInBytes);

#if defined(_MSC_VER) || defined(__MINGW32__)
	storageFileHandle = INVALID_HANDLE_VALUE;
#elif defined(__GNUC__)
	storageFileHandle = -1;
#endif
	return;
}

// DWORD == int
int OpenStorageFile(char *filename)
{
	printf("Opening file...\n");

#if defined(_MSC_VER) || defined(__MINGW32__)
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
	storageFileHandle = open(filename, O_CREAT | O_RDWR, S_IRUSR | S_IWUSR);
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

	// по-крайней мере - минимальный блок для линков + сервисный блок
	if (storageFileSizeInBytes < storageFileMinSizeInBytes) {
		printf("enlarge\n");
		storageFileSizeInBytes = storageFileMinSizeInBytes;
	}

	// если блок линков выравнен неправильно (не кратен базовому размеру блока), выравниваем "вверх"
	if (((storageFileSizeInBytes - serviceBlockSizeInBytes) % baseBlockSizeInBytes) > 0)
		storageFileSizeInBytes = (((storageFileSizeInBytes - serviceBlockSizeInBytes) / baseBlockSizeInBytes) * baseBlockSizeInBytes) + baseBlockSizeInBytes;

	printf("storageFileSizeInBytes = %llu\n",
	       (long long unsigned int)storageFileSizeInBytes);

	printf("File %s opened.\n\n", filename);

	return 0;
}

// стандартный тип ошибки - int
int SetStorageFileMemoryMapping()
{
	printf("Setting memory mapping of storage file...\n");

#if defined(_MSC_VER) || defined(__MINGW32__)
	// см. MSDN "CreateFileMapping function", http://msdn.microsoft.com/en-us/library/windows/desktop/aa366537%28v=vs.85%29.aspx
	storageFileMappingHandle = CreateFileMapping(storageFileHandle, NULL, PAGE_READWRITE, 0, storageFileSizeInBytes, NULL);
	if (storageFileMappingHandle == NULL)
	{
		unsigned long error = GetLastError();
		printf("Mapping creation failed. Error code: %lu.\n\n", error);
		return error;
	}

	// аналог mmap(),
	// см. MSDN "MapViewOfFileEx function", http://msdn.microsoft.com/en-us/library/windows/desktop/aa366763%28v=vs.85%29.aspx
	// см. MSDN "MapViewOfFile function", http://msdn.microsoft.com/en-us/library/windows/desktop/aa366761%28v=vs.85%29.aspx
	// hFileMappingObject [in] A handle to a file mapping object. The CreateFileMapping and OpenFileMapping functions return this handle.
	pointerToMappedRegion = MapViewOfFileEx(storageFileMappingHandle, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0, pointerToMappedRegion);
	if (pointerToMappedRegion == NULL) 
	{
		unsigned long error = GetLastError();
		printf("Mapping view set failed. Error code: %lu.\n\n", error);
		return error;
	}
#elif defined(__GNUC__)
	// см. также под Linux, MAP_POPULATE
	// см. также mmap64() (size_t?)
	ftruncate(storageFileHandle, storageFileSizeInBytes);
	pointerToMappedRegion = mmap(NULL, storageFileSizeInBytes, PROT_READ | PROT_WRITE, MAP_SHARED, storageFileHandle, 0);

	if (pointerToMappedRegion == MAP_FAILED)
	{
		int error = errno;
		printf("Mapping view set failed. Error code: %d.\n\n", error);
		return error;
	}
	else {
		printf("mmap() passed\n");
	}
#endif

	pointerToBaseLinksMaxSize = (int*)((unsigned char *)pointerToMappedRegion + 8 + 4);
	pointerToPointerToBaseLinks = (Link**)((unsigned char *)pointerToMappedRegion + 8 + 4 + 4);

	pointerToLinksMaxSize = (uint64_t *)((unsigned char *)pointerToMappedRegion + serviceBlockSizeInBytes);
	pointerToLinksSize = (uint64_t *)((unsigned char *)pointerToMappedRegion + serviceBlockSizeInBytes + 8);
	/* Далее следует неиспользуемый блок памяти, с размером (sizeof(Link) - 16) */
	pointerToUnusedMarker = (Link*)((unsigned char *)pointerToMappedRegion + serviceBlockSizeInBytes + sizeof(Link));
	pointerToLinks = (Link*)((unsigned char *)pointerToMappedRegion + serviceBlockSizeInBytes + 2 * sizeof(Link));

	printf("pointerToBaseLinksMaxSize = %d\n", *pointerToBaseLinksMaxSize);
	printf("pointerToLinksMaxSize = %llu\n",
	       (long long unsigned int)*pointerToLinksMaxSize);
	printf("pointerToLinksSize = %llu\n",
	       (long long unsigned int)*pointerToLinksSize);


	// Выполняем первоначальную инициализацию и валидацию основных вспомогательных счётчиков и значений



	if (*pointerToBaseLinksMaxSize == 0)
		*pointerToBaseLinksMaxSize = (baseLinksSizeInBytes - 4) / 8; // ? почему так

	if (*pointerToLinksMaxSize == 0)
		*pointerToLinksMaxSize = (storageFileSizeInBytes - serviceBlockSizeInBytes - 2 * sizeof(Link)) / sizeof(Link);
		// <- число линков, после Marker, объем "памяти" (в линках)
	// если переменная установлена неправильно, исправляем
	else if (*pointerToLinksMaxSize != (storageFileSizeInBytes - serviceBlockSizeInBytes - 2 * sizeof(Link)) / sizeof(Link))
		*pointerToLinksMaxSize = (storageFileSizeInBytes - serviceBlockSizeInBytes - 2 * sizeof(Link)) / sizeof(Link);

	// число линков почему-то превышает заданный размер (может быть, реакция должна быть не такой?)
	if (*pointerToLinksSize > *pointerToLinksMaxSize)
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

int CloseStorageFile()
{
	printf("Closing storage file...\n");

	// При освобождении лишнего места, можно уменьшать размер файла, для этого используется функция SetEndOfFile(fh); 
	// По завершению работы с файлом можно устанавливать ограничение на размер реальных данных файла	SetFileValidData(fh,newFileLen); 

#if defined(_MSC_VER) || defined(__MINGW32__)
	if (storageFileHandle == INVALID_HANDLE_VALUE) // т.к. например STDIN_FILENO == 0 - для stdin (под Linux)
	{
		// убрал принудительный выход, так как даже в случае неправильного дескриптора, его можно попытаться закрыть
//		unsigned long error = -1;
		printf("Storage file is not open or already closed.\n\n");
//		return error;
	}

	CloseHandle(storageFileHandle);
	storageFileHandle = INVALID_HANDLE_VALUE;
	storageFileSizeInBytes = 0;
#elif defined(__GNUC__)
	if (storageFileHandle == -1)
	{
		printf("Storage file is not open or already closed.\n\n");
		return -1;
	}

	close(storageFileHandle);
	storageFileHandle = -1;
	storageFileSizeInBytes = 0;
#endif

	printf("Storage file closed.\n\n");
		
	return 0;
}

unsigned long EnlargeStorageFile()
{
#if defined(_MSC_VER) || defined(__MINGW32__)
	if (storageFileHandle == INVALID_HANDLE_VALUE)
#elif defined(__GNUC__)
	if (storageFileHandle == -1)
#endif
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

		storageFileSizeInBytes += baseBlockSizeInBytes;

		// там происходит увеличение через ftruncate(), под Linux
		error = SetStorageFileMemoryMapping();
		if(error != 0)
			return error;
	}

	return 0;
}

unsigned long ShrinkStorageFile()
{
#if defined(_MSC_VER) || defined(__MINGW32__)
	if (storageFileHandle == INVALID_HANDLE_VALUE)
#elif defined(__GNUC__)
	if (storageFileHandle == -1)
#endif
	{
		unsigned long error = -1;
		printf("Storage file is not open.\n");
		return error;
	}

	if (storageFileSizeInBytes > storageFileMinSizeInBytes)
	{
		unsigned long error = 0;
		unsigned long long linksTableNewMaxSize = (storageFileSizeInBytes - baseBlockSizeInBytes - serviceBlockSizeInBytes - 2 * sizeof(Link)) / sizeof(Link);

		if (*pointerToLinksSize < linksTableNewMaxSize)
		{
			error = ResetStorageFileMemoryMapping();
			if(error != 0)
				return error;

#if defined(_MSC_VER) || defined(__MINGW32__)
			{
				LARGE_INTEGER distanceToMoveFilePointer;
				distanceToMoveFilePointer.QuadPart = -((long long)baseBlockSizeInBytes);

				storageFileSizeInBytes -= baseBlockSizeInBytes;

				SetFilePointerEx(storageFileHandle, distanceToMoveFilePointer, NULL, FILE_END);
				SetEndOfFile(storageFileHandle);
			}
#elif defined(__GNUC__)
				storageFileSizeInBytes -= baseBlockSizeInBytes;
#endif

			// уменьшение через ftruncate()
			error = SetStorageFileMemoryMapping();
			if(error != 0)
				return error;
		}
	}

	return 0;
}


unsigned long ResetStorageFileMemoryMapping()
{
	printf("Resetting memory mapping of storage file...\n");

#if defined(_MSC_VER) || defined(__MINGW32__)
	if (storageFileHandle == INVALID_HANDLE_VALUE)
#elif defined(__GNUC__)
	if (storageFileHandle == -1)
#endif
	{
		unsigned long error = -1;
		printf("Memory mapping of storage file is not set or already reset.\n");
		return error;
	}

	PrintLinksTableSize();

#if defined(_MSC_VER) || defined(__MINGW32__)
	UnmapViewOfFile (pointerToMappedRegion);
	CloseHandle(storageFileMappingHandle);
	storageFileMappingHandle = INVALID_HANDLE_VALUE;
#elif defined(__GNUC__)
	munmap(pointerToMappedRegion, storageFileSizeInBytes);
//	storageFileHandle = -1; // некорректно так делать
#endif

	printf("Memory mapping of storage file is reset.\n\n");

	return 0;
}

/***  Работа с линками  ***/

Link* AllocateFromUnusedLinks()
{
	Link* unusedLink = pointerToUnusedMarker->FirstRefererByLinker;
	DetachLinkFromMarker(unusedLink, pointerToUnusedMarker);
	return unusedLink;
}

// пока что программа - однопоточная, не надо использовать mutex'и
Link* AllocateFromFreeLinks()
{
	Link* freeLink;

	if (*pointerToLinksMaxSize == *pointerToLinksSize)
		EnlargeStorageFile();

	freeLink = pointerToLinks + *pointerToLinksSize;
	++*pointerToLinksSize;
	return freeLink;
}

Link* AllocateLink()
{
	if (pointerToUnusedMarker->FirstRefererByLinker != null)
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
		Link* lastUsedLink = pointerToLinks + *pointerToLinksSize - 1;

		if (link < lastUsedLink)
		{
			AttachLinkToMarker(link, pointerToUnusedMarker);
		}
		else if(link == lastUsedLink)
		{
			--*pointerToLinksSize;

			while((--lastUsedLink)->Linker == pointerToUnusedMarker)
			{
				DetachLinkFromMarker(lastUsedLink, pointerToUnusedMarker);
				--*pointerToLinksSize;
			}

			ShrinkStorageFile(); // Размер будет уменьшен, только если допустимо
		}
	}
}

void WalkThroughAllLinks(func func_)
{
	Link *currentLink = pointerToLinks;
	Link* lastLink = pointerToLinks + *pointerToLinksSize - 1;

	do
	{
		if (currentLink->Linker != pointerToUnusedMarker)
		{
			func_(currentLink);
		}
	}
	while(++currentLink <= lastLink);
}

int WalkThroughLinks(func func_)
{
	Link *currentLink = pointerToLinks;
	Link* lastLink = pointerToLinks + *pointerToLinksSize - 1;

	do
	{
		if (currentLink->Linker != pointerToUnusedMarker)
		{
			if(!func_(currentLink)) return false;
		}
	}
	while(++currentLink <= lastLink);
	
	return true;
}

Link* GetMappedLink(int index)
{
	if (index < *pointerToBaseLinksMaxSize)
		return pointerToPointerToBaseLinks[index];
	else
		return null;
}

void SetMappedLink(int index, Link* link)
{
	if (index < *pointerToBaseLinksMaxSize)
		pointerToPointerToBaseLinks[index] = link;
}
