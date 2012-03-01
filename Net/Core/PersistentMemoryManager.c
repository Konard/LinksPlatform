#if defined(_MFC_VER) || defined(__MINGW32__)
#include <windows.h>

// подключение необходимых заголовочных файлов для Linux
#elif defined(__GNUC__)
// for 64-bit files -- работать с большими файлами
#define _XOPEN_SOURCE 700
#include <unistd.h>
// open() -- открыть файл базы данных
#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
// errno -- откуда брать коды ошибок
#include <errno.h>
// mmap()... -- само собой разумеется
#include <sys/mman.h>
// NULL -- вместо неправильного, null
#include <malloc.h>
#endif

#include <stdio.h>
#include "Common.h"
#include "Link.h"
#include "PersistentMemoryManager.h"

// Дескриптор файла базы данных и дескриптор объекта отображения (mmap)
#if defined(_MFC_VER) || defined(__MINGW32__)
HANDLE		storageFileHandle;
HANDLE		storageFileMappingHandle;
#elif defined(__GNUC__)
int			storageFileHandle; // для open()
// для mmap() под Linux дескриптор не используется, достаточно адреса + размера области
#endif
uint64_t	storageFileSizeInBytes; // <- off_t


// Константы, рассчитываемые при запуске приложения
int			currentMemoryPageSizeInBytes;	// Размер страницы в операционной системе
int			serviceBlockSizeInBytes;	// Размер сервисных данных, (две страницы)
int			baseLinksSizeInBytes;
uint64_t	baseBlockSizeInBytes;		// Базовый размер блока данных (является минимальным размером файла, а также шагом при росте этого файла)
uint64_t	storageFileMinSizeInBytes;

void*		pointerToMappedRegion; 		// указатель на начало региона памяти - результата mmap()

uint32_t*	pointerToBaseLinksMaxSize;
uint64_t*	pointerToBaseLinks;			// инициализируется в SetStorageFileMemoryMapping()

uint64_t*	pointerToLinksMaxSize;
uint64_t*	pointerToLinksSize;

/* Не используемый блок памяти, с размером (sizeof(Link) - 16) */
Link*		pointerToUnusedMarker;		// инициализируется в SetStorageFileMemoryMapping()
Link*		pointerToLinks;				// здесь хранятся линки, инициализируется в SetStorageFileMemoryMapping()


/***  Работа с памятью  ***/

void PrintLinksTableSize()
{
#if defined(_MFC_VER) || defined(__MINGW32__)
	printf("Links table size: %I64d links, %I64d bytes.\n", *pointerToLinksSize, *pointerToLinksSize * sizeof(Link));
#elif defined(__GNUC__)
	printf("Links table size: %llu links, %llu bytes.\n",
	  (long long unsigned int)(*pointerToLinksSize),
	  (long long unsigned int)(*pointerToLinksSize * sizeof(Link)));
#endif
}

void InitPersistentMemoryManager()
{

#if defined(_MFC_VER) || defined(__MINGW32__)

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

	baseLinksSizeInBytes = serviceBlockSizeInBytes - 12; // ??? Почему так
	baseBlockSizeInBytes = currentMemoryPageSizeInBytes * 256 * 4 * sizeof(Link); // ~ 512 mb

	storageFileMinSizeInBytes = serviceBlockSizeInBytes + baseBlockSizeInBytes;

	printf("storageFileMinSizeInBytes = %llu\n",
	       (long long unsigned int)storageFileMinSizeInBytes);

#if defined(_MFC_VER) || defined(__MINGW32__)
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
		printf("enlarge storage file ...\n");
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

#if defined(_MFC_VER) || defined(__MINGW32__)
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

	// pointer to bytes
	pointerToBaseLinksMaxSize = (uint32_t*)((char *)pointerToMappedRegion + sizeof(uint64_t) + sizeof(uint32_t));
	pointerToBaseLinks = (uint64_t *)((char *)pointerToMappedRegion + sizeof(uint64_t) + sizeof(uint32_t) + sizeof(uint32_t)); // ??? правильное понимание этого указателя?

	pointerToLinksMaxSize = (uint64_t *)((char *)pointerToMappedRegion + serviceBlockSizeInBytes);
	pointerToLinksSize = (uint64_t *)((char *)pointerToMappedRegion + serviceBlockSizeInBytes + sizeof(uint64_t));
	/* Далее следует неиспользуемый блок памяти, с размером (sizeof(Link) - 16) */
	pointerToUnusedMarker = (Link*)((char *)pointerToMappedRegion + serviceBlockSizeInBytes + sizeof(Link));
	pointerToLinks = (Link*)((char *)pointerToMappedRegion + serviceBlockSizeInBytes + sizeof(Link)); // чтобы Links[0] == UnusedMarker
//	pointerToLinks = (Link*)((char *)pointerToMappedRegion + serviceBlockSizeInBytes + 2 * sizeof(Link));

	printf("pointerToBaseLinksMaxSize = %d\n", *pointerToBaseLinksMaxSize);
	printf("pointerToLinksMaxSize = %llu\n",
	       (long long unsigned int)*pointerToLinksMaxSize);
	printf("pointerToLinksSize = %llu\n",
	       (long long unsigned int)*pointerToLinksSize);


	// Выполняем первоначальную инициализацию и валидацию основных вспомогательных счётчиков и значений

	if (*pointerToBaseLinksMaxSize == 0)
		*pointerToBaseLinksMaxSize = (baseLinksSizeInBytes - 4) / 8; // ??? выяснить - почему так

	// Links == UnusedMarker
	if (*pointerToLinksMaxSize == 0)
		*pointerToLinksMaxSize = (storageFileSizeInBytes - serviceBlockSizeInBytes - sizeof(Link)) / sizeof(Link);
		// <- число линков, после Marker, объем "памяти" (в линках)
	// если переменная установлена неправильно, исправляем
	else if (*pointerToLinksMaxSize != (storageFileSizeInBytes - serviceBlockSizeInBytes - sizeof(Link)) / sizeof(Link))
		*pointerToLinksMaxSize = (storageFileSizeInBytes - serviceBlockSizeInBytes - sizeof(Link)) / sizeof(Link);

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

#if defined(_MFC_VER) || defined(__MINGW32__)
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
#if defined(_MFC_VER) || defined(__MINGW32__)
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
#if defined(_MFC_VER) || defined(__MINGW32__)
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

#if defined(_MFC_VER) || defined(__MINGW32__)
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

#if defined(_MFC_VER) || defined(__MINGW32__)
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

#if defined(_MFC_VER) || defined(__MINGW32__)
	UnmapViewOfFile (pointerToMappedRegion);
	CloseHandle(storageFileMappingHandle);
	storageFileMappingHandle = INVALID_HANDLE_VALUE;
#elif defined(__GNUC__)
	munmap(pointerToMappedRegion, storageFileSizeInBytes);
//	storageFileHandle = -1; // некорректно так делать, а дескриптора для mmapped region нету
#endif

	printf("Memory mapping of storage file is reset.\n\n");

	return 0;
}

/***  Работа с линками  ***/

uint64_t AllocateFromUnusedLinks()
{
	uint64_t unusedLinkIndex = GetByLinkerIndex(LINK_0); // ??? правильно?
	DetachLinkFromUnusedMarker(unusedLinkIndex); // переименовали функцию, pointerToUnusedMarker уже не передаём
	return unusedLinkIndex;
}

// пока что программа - однопоточная, не надо использовать mutex'и
uint64_t AllocateFromFreeLinks()
{
	if (*pointerToLinksMaxSize <= *pointerToLinksSize) // более корректно: <=, чем ==
		EnlargeStorageFile();
	uint64_t freeLinkIndex = (*pointerToLinksSize); // первый свободный элемент в таблице
	++(*pointerToLinksSize);
	return freeLinkIndex; // после return - увеличиваем
}

uint64_t AllocateLink()
{
	if (GetByLinkerIndex(LINK_0) != LINK_0) // ??? корректно ли
		return AllocateFromUnusedLinks();
	else
		return AllocateFromFreeLinks();	
}

void FreeLink(uint64_t linkIndex)
{
	DetachLink(linkIndex);

    while (GetBySourceIndex(linkIndex) != LINK_0) FreeLink(GetBySourceIndex(linkIndex));
    while (GetByLinkerIndex(linkIndex) != LINK_0) FreeLink(GetByLinkerIndex(linkIndex));
    while (GetByTargetIndex(linkIndex) != LINK_0) FreeLink(GetByTargetIndex(linkIndex));

	{
		uint64_t lastUsedLinkIndex = *pointerToLinksSize - 1; // pointerToLinks и так смещается на -1, поэтому -1 убираем

		if (linkIndex < lastUsedLinkIndex)
		{
			AttachLinkToUnusedMarker(linkIndex); // убран указатель на Marker, так как функция принимала только такой аргумент (второй)
		}
		else if(linkIndex == lastUsedLinkIndex)
		{
			--*pointerToLinksSize;

			while(GetLinkerIndex(--lastUsedLinkIndex) == LINK_0) // ?
			{
				DetachLinkFromUnusedMarker(lastUsedLinkIndex);
				--*pointerToLinksSize;
			}

			ShrinkStorageFile(); // Размер будет уменьшен, только если допустимо
		}
	}
}

void WalkThroughAllLinks(func func_)
{
	uint64_t currentLinkIndex = LINK_0; // ??? правильный ли старт
	uint64_t lastLinkIndex = LINK_0 + *pointerToLinksSize - 1;

	do
	{
		if (GetLinkerIndex(currentLinkIndex) != LINK_0) // ? корректна ли замена
		{
			func_(currentLinkIndex);
		}
	}
	while(++currentLinkIndex <= lastLinkIndex);
}

int WalkThroughLinks(func func_)
{
	uint64_t currentLinkIndex = LINK_0; // ??? правильный ли старт
	uint64_t lastLinkIndex = LINK_0 + *pointerToLinksSize - 1;

	do
	{
		if (GetLinkerIndex(currentLinkIndex) != LINK_0) // ?
		{
			if(!func_(currentLinkIndex)) return false;
		}
	}
	while(++currentLinkIndex <= lastLinkIndex);
	
	return true;
}


// работа с базовыми линками
uint64_t GetBaseLink(uint32_t index)
{
	if (index < *pointerToBaseLinksMaxSize)
		return pointerToBaseLinks[index]; // возвращает индекс линка, linkIndex
	else
		return LINK_0; // вместо Link * == NULL
}

// базовых линков не должно быть много, поэтому - int
void SetBaseLink(uint32_t index, uint64_t linkIndex)
{
	if (index < *pointerToBaseLinksMaxSize)
		pointerToBaseLinks[index] = linkIndex; // может быть 0 (соответствует NULL)
}


// работа с основными линками
Link* GetLink(uint64_t linkIndex)
{
	if (linkIndex < *pointerToLinksMaxSize) // ? Почему -Max
		return &(pointerToLinks[linkIndex]);
	else {
		printf("linkIndex = %llu\n", (long long unsigned int)linkIndex);
		return NULL; // корректный указатель из malloc.h
	}
}

// SetLink, SetSource, ... не нужны - так как поломают целостность ассоц. сети

// GetSource(0) == 0 - это очень важно
// GetSource(X) == 0, где X - удаленная или неправильная связь - тоже очень важно
uint64_t GetSourceIndex(uint64_t linkIndex)
{
	Link* link = GetLink(linkIndex);
	return (link == NULL) ? LINK_0 : link->Source;
}

uint64_t GetTargetIndex(uint64_t linkIndex)
{
	Link* link = GetLink(linkIndex);
	return (link == NULL) ? LINK_0 : link->Target;
}

// LinkerLink index
uint64_t GetLinkerIndex(uint64_t linkIndex)
{
	Link* link = GetLink(linkIndex);
	return (link == NULL) ? LINK_0 : link->Linker;
}

// By ...
uint64_t GetBySourceIndex(uint64_t linkIndex)
{
	Link* link = GetLink(linkIndex);
	return (link == NULL) ? LINK_0 : link->BySource;
}

uint64_t GetByTargetIndex(uint64_t linkIndex)
{
	Link* link = GetLink(linkIndex);
	return (link == NULL) ? LINK_0 : link->ByTarget;
}

uint64_t GetByLinkerIndex(uint64_t linkIndex)
{
	Link* link = GetLink(linkIndex);
	return (link == NULL) ? LINK_0 : link->ByLinker;
}

// LeftBy ...
uint64_t GetLeftBySourceIndex(uint64_t linkIndex)
{
	Link* link = GetLink(linkIndex);
	return (link == NULL) ? LINK_0 : link->LeftBySource;
}

uint64_t GetLeftByTargetIndex(uint64_t linkIndex)
{
	Link* link = GetLink(linkIndex);
	return (link == NULL) ? LINK_0 : link->LeftByTarget;
}

uint64_t GetLeftByLinkerIndex(uint64_t linkIndex)
{
	Link* link = GetLink(linkIndex);
	return (link == NULL) ? LINK_0 : link->LeftByLinker;
}

// RightBy ...
uint64_t GetRightBySourceIndex(uint64_t linkIndex)
{
	Link* link = GetLink(linkIndex);
	return (link == NULL) ? LINK_0 : link->RightBySource;
}

uint64_t GetRightByTargetIndex(uint64_t linkIndex)
{
	Link* link = GetLink(linkIndex);
	return (link == NULL) ? LINK_0 : link->RightByTarget;
}

uint64_t GetRightByLinkerIndex(uint64_t linkIndex)
{
	Link* link = GetLink(linkIndex);
	return (link == NULL) ? LINK_0 : link->RightByLinker;
}
