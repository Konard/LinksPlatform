// Менеджер памяти (memory manager).

#include "Common.h"

#if defined(WINDOWS)
#include <windows.h>
#elif defined(LINUX)
// for 64-bit files
#define _XOPEN_SOURCE 700
// for memset
#include <string.h>
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

#include "Link.h"
#include "PersistentMemoryManager.h"

// Дескриптор файла базы данных и дескриптор объекта отображения (map)
#if defined(WINDOWS)
HANDLE              storageFileHandle;
HANDLE              storageFileMappingHandle;
#elif defined(LINUX)
signed_integer      storageFileHandle;                  // для open()
#endif
int64_t             storageFileSizeInBytes;             // Текущий размер файла.

void*               pointerToMappedRegion;              // указатель на начало региона памяти - результата mmap()

// Константы, рассчитываемые при запуске приложения
int64_t             currentMemoryPageSizeInBytes;       // Размер страницы в операционной системе. Инициализируется в InitPersistentMemoryManager();
int64_t             serviceBlockSizeInBytes;            // Размер сервисных данных, (две страницы). Инициализируется в InitPersistentMemoryManager();
int64_t             baseLinksSizeInBytes;               // Размер массива базовых (привязанных) связей.
int64_t             baseBlockSizeInBytes;               // Базовый размер блока данных (шаг роста файла базы данных)
int64_t             storageFileMinSizeInBytes;          // Минимально возможный размер файла базы данных (Базовый размер блока (шага) + размер сервисного блока)


uint64_t*           pointerToDataSeal;                  // Указатель на уникальную печать, если она установлена, значит база данных открывается во второй или более раз.
uint64_t*           pointerToLinkIndexSize;             // Указатель на размер одного индекса связи.
uint64_t*           pointerToMappingLinksMaxSize;       // Указатель на максимальный размер массива базовых (привязанных) связей.
link_index*         pointerToPointerToMappingLinks;     // Указатель на начало массива базовых (привязанных) связей. Инициализируется в SetStorageFileMemoryMapping().
link_index*         pointerToLinksMaxSize;              // Указатель на максимальный размер массива связей.
link_index*         pointerToLinksSize;                 // Указатель на текущий размер массива связей.
Link*               pointerToLinks;                     // Указатель на начало массива связей. Инициализируется в SetStorageFileMemoryMapping().

Link*               pointerToUnusedMarker;              // Инициализируется в SetStorageFileMemoryMapping()

void PrintLinksDatabaseSize()
{
#ifdef DEBUG
    printf("Links database size: %" PRIu64 " links, %" PRIu64 " bytes for links. Service block size (bytes): %" PRIu64 ".\n",
        (uint64_t)(*pointerToLinksSize),
        (uint64_t)(*pointerToLinksSize * sizeof(Link)),
        (uint64_t)serviceBlockSizeInBytes);
#endif
}

bool ExistsLink(Link* link)
{
    return link && pointerToLinks != link && link->LinkerIndex; //link->SourceIndex && link->LinkerIndex && link->TargetIndex;
}

bool ExistsLinkIndex(link_index linkIndex)
{
    return ExistsLink(GetLink(linkIndex));
}

bool IsNullLinkEmpty()
{
    return !pointerToLinks->SourceIndex && !pointerToLinks->LinkerIndex && !pointerToLinks->TargetIndex;
}

Link* GetLink(link_index linkIndex)
{
    return pointerToLinks + linkIndex;
}

link_index GetLinkIndex(Link* link)
{
    return link - pointerToLinks;
}

unsigned_integer GetLinksCount()
{
    return *pointerToLinksSize - 1;
}

unsigned_integer GetCurrentSystemPageSize()
{
#if defined(WINDOWS)

    //  typedef struct _SYSTEM_INFO {
    //      union {
    //          DWORD  dwOemId;
    //          struct {
    //              WORD wProcessorArchitecture;
    //              WORD wReserved;
    //          };
    //      };
    //      DWORD     dwPageSize;
    //      LPVOID    lpMinimumApplicationAddress;
    //      LPVOID    lpMaximumApplicationAddress;
    //      DWORD_PTR dwActiveProcessorMask;
    //      DWORD     dwNumberOfProcessors;
    //      DWORD     dwProcessorType;
    //      DWORD     dwAllocationGranularity;
    //      WORD      wProcessorLevel;
    //      WORD      wProcessorRevision;
    //  } SYSTEM_INFO;

    SYSTEM_INFO info; // см. http://msdn.microsoft.com/en-us/library/windows/desktop/ms724958%28v=vs.85%29.aspx
    GetSystemInfo(&info);
    return info.dwPageSize;

#elif defined(LINUX)

    long pageSize = sysconf(_SC_PAGESIZE);
    return pageSize;

#endif
}

void ResetStorageFile()
{
#if defined(WINDOWS)
    storageFileHandle = INVALID_HANDLE_VALUE;
#elif defined(LINUX)
    storageFileHandle = -1;
#endif
    storageFileSizeInBytes = 0;
}

bool IsStorageFileOpened()
{
#if defined(WINDOWS)
    return storageFileHandle != INVALID_HANDLE_VALUE;
#elif defined(LINUX)
    return storageFileHandle != -1;
#endif
}

signed_integer EnsureStorageFileOpened()
{
    if (!IsStorageFileOpened())
        return Error("Storage file is not open.");
    return SUCCESS_RESULT;
}

signed_integer EnsureStorageFileClosed()
{
    if (IsStorageFileOpened())
        return Error("Storage file is not closed.");
    return SUCCESS_RESULT;
}

signed_integer ResetStorageFileMapping()
{
#if defined(WINDOWS)
    storageFileMappingHandle = INVALID_HANDLE_VALUE;
    pointerToMappedRegion = NULL;
#elif defined(LINUX)
    pointerToMappedRegion = MAP_FAILED;
#endif
    return (signed_integer)UINT64_MAX;
}

bool IsStorageFileMapped()
{
#if defined(WINDOWS)
    return storageFileMappingHandle != INVALID_HANDLE_VALUE && pointerToMappedRegion != NULL;
#elif defined(LINUX)
    return pointerToMappedRegion != MAP_FAILED;
#endif
}

signed_integer EnsureStorageFileMapped()
{
    if (!IsStorageFileMapped())
        return Error("Storage file is not mapped.");
    return SUCCESS_RESULT;
}

signed_integer EnsureStorageFileUnmapped()
{
    if (IsStorageFileMapped())
        return Error("Storage file already mapped.");
    return SUCCESS_RESULT;
}

void InitPersistentMemoryManager()
{
    currentMemoryPageSizeInBytes = GetCurrentSystemPageSize();
    serviceBlockSizeInBytes = currentMemoryPageSizeInBytes * 2;

    baseLinksSizeInBytes = serviceBlockSizeInBytes - sizeof(uint64_t) * 3 - sizeof(link_index) * 2;
    baseBlockSizeInBytes = currentMemoryPageSizeInBytes * 256 * 4 * sizeof(Link); // ~ 512 mb

    storageFileMinSizeInBytes = serviceBlockSizeInBytes + baseBlockSizeInBytes;

#ifdef DEBUG
    printf("storageFileMinSizeInBytes = %" PRIu64 "\n", (uint64_t)storageFileMinSizeInBytes);
#endif

    ResetStorageFile();
    ResetStorageFileMapping();

    return;
}

signed_integer OpenStorageFile(char* filename)
{
    if (failed(EnsureStorageFileClosed()))
        return ERROR_RESULT;

    DebugInfo("Opening file...");

#if defined(WINDOWS)
    // см. MSDN "CreateFile function", http://msdn.microsoft.com/en-us/library/windows/desktop/aa363858%28v=vs.85%29.aspx
    storageFileHandle = CreateFile(filename, GENERIC_READ | GENERIC_WRITE, 0, NULL, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
    if (storageFileHandle == INVALID_HANDLE_VALUE)
        // см. MSDN "GetLastError function", http://msdn.microsoft.com/en-us/library/windows/desktop/ms679360%28v=vs.85%29.aspx
        return ErrorWithCode("Failed to open file.", GetLastError());

    // см. MSDN "GetFileSize function", http://msdn.microsoft.com/en-us/library/windows/desktop/aa364955%28v=vs.85%29.aspx
    // не знаю, как поправить здесь:
    // warning: dereferencing type-punned pointer will break strict-aliasing rules
    *((LPDWORD)&storageFileSizeInBytes) = GetFileSize(storageFileHandle, (LPDWORD)&storageFileSizeInBytes + 1);
    if (storageFileSizeInBytes == INVALID_FILE_SIZE)
        return ErrorWithCode("Failed to get file size.", GetLastError());

#elif defined(LINUX)
    storageFileHandle = open(filename, O_CREAT | O_RDWR, S_IRUSR | S_IWUSR);
    if (storageFileHandle == -1)
        return ErrorWithCode("Failed to open file.", errno);

    struct stat statbuf;
    if (fstat(storageFileHandle, &statbuf) != 0)
        return ErrorWithCode("Failed to get file size.", errno);

    storageFileSizeInBytes = statbuf.st_size; // ? uint64_t = off_t
#endif

#ifdef DEBUG
    printf("storageFileSizeInBytes = %" PRIu64 "\n", (uint64_t)storageFileSizeInBytes);

    printf("File %s opened.\n\n", filename);
#endif

    return SUCCESS_RESULT;
}

// Используется storageFileHandle и storageFileSizeInBytes для установки нового размера
signed_integer ResizeStorageFile()
{
    if (succeeded(EnsureStorageFileOpened()))
    {
        if (succeeded(EnsureStorageFileUnmapped()))
        {
#if defined(WINDOWS)
            LARGE_INTEGER distanceToMoveFilePointer = { 0 };
            LARGE_INTEGER currentFilePointer = { 0 };
            if (!SetFilePointerEx(storageFileHandle, distanceToMoveFilePointer, &currentFilePointer, FILE_CURRENT))
                return ErrorWithCode("Failed to get current file pointer.", GetLastError());

            distanceToMoveFilePointer.QuadPart = storageFileSizeInBytes - currentFilePointer.QuadPart;

            if (!SetFilePointerEx(storageFileHandle, distanceToMoveFilePointer, NULL, FILE_END))
                return ErrorWithCode("Failed to set file pointer.", GetLastError());
            if (!SetEndOfFile(storageFileHandle))
                return ErrorWithCode("Failed to set end of file.", GetLastError());
#elif defined(LINUX)
            // см. также под Linux, MAP_POPULATE
            // см. также mmap64() (size_t?)
            if (ftruncate(storageFileHandle, storageFileSizeInBytes) == -1)
                return ErrorWithCode("Failed to resize file.", errno);
#endif

            return SUCCESS_RESULT;
        }
    }

    return ERROR_RESULT;
}

signed_integer SetStorageFileMemoryMapping()
{
    if (failed(EnsureStorageFileOpened()))
        return ERROR_RESULT;

    if (failed(EnsureStorageFileUnmapped()))
        return ERROR_RESULT;

    DebugInfo("Setting memory mapping of storage file..");

    // по-крайней мере - минимальный блок для линков + сервисный блок
    if (storageFileSizeInBytes < storageFileMinSizeInBytes)
        storageFileSizeInBytes = storageFileMinSizeInBytes;

    // если блок линков выравнен неправильно (не кратен базовому размеру блока), выравниваем "вверх"
    if (((storageFileSizeInBytes - serviceBlockSizeInBytes) % baseBlockSizeInBytes) > 0)
        storageFileSizeInBytes = (((storageFileSizeInBytes - serviceBlockSizeInBytes) / baseBlockSizeInBytes) * baseBlockSizeInBytes) + storageFileMinSizeInBytes;

    ResizeStorageFile();

#if defined(WINDOWS)
    // см. MSDN "CreateFileMapping function", http://msdn.microsoft.com/en-us/library/windows/desktop/aa366537%28v=vs.85%29.aspx
    storageFileMappingHandle = CreateFileMapping(storageFileHandle, NULL, PAGE_READWRITE, 0, (DWORD)storageFileSizeInBytes, NULL);
    if (storageFileMappingHandle == INVALID_HANDLE_VALUE)
        return ErrorWithCode("Mapping creation failed.", GetLastError());

    // аналог mmap(),
    // см. MSDN "MapViewOfFileEx function", http://msdn.microsoft.com/en-us/library/windows/desktop/aa366763%28v=vs.85%29.aspx
    // см. MSDN "MapViewOfFile function", http://msdn.microsoft.com/en-us/library/windows/desktop/aa366761%28v=vs.85%29.aspx
    // hFileMappingObject [in] A handle to a file mapping object. The CreateFileMapping and OpenFileMapping functions return this handle.
    pointerToMappedRegion = MapViewOfFileEx(storageFileMappingHandle, FILE_MAP_READ | FILE_MAP_WRITE, 0, 0, 0, pointerToMappedRegion);
    if (pointerToMappedRegion == NULL)
        return ErrorWithCode("Failed to set map view of file.", GetLastError());

#elif defined(LINUX)
    pointerToMappedRegion = mmap(NULL, storageFileSizeInBytes, PROT_READ | PROT_WRITE, MAP_SHARED, storageFileHandle, 0);

    if (pointerToMappedRegion == MAP_FAILED)
        return ErrorWithCode("Failed to set map view of file.", errno);
#endif

    //       Storage File Structure
    //    ============================
    //   | Service Block              | *
    //   | DataSeal            !64bit | |
    //   | LinkIndexSize       !64bit | |
    //   | MappingLinksMaxSize !64bit | |
    //   | LinksMaxSize         64bit | |
    //   | LinksActualSize      64bit | | 2 * (System Page Size)
    //   |              *             | |
    //   |   Base       |    Link     | |
    //   |  (Mapped)    |  indicies   | |
    //   |              *             | *
    //   | Links Block                | *
    //   |              *             | | Min after save: 1 * (Link Size) // Needed for null link (unused link marker)
    //   |   Actual     |    Link     | | Min on open: (BaseBlockSizeInBytes) // Grow step (default is 512 mb)
    //   |   Data       |  Structures | |
    //   |              |             | |
    //   |              *             | *
    //    ============================
    //    ! means it is always that size (does not depend on link_index size)

    void* pointers[7] = {
        // Service Block
        (char*)pointerToMappedRegion + sizeof(uint64_t) * 0, // 0
        (char*)pointerToMappedRegion + sizeof(uint64_t) * 1, // 1
        (char*)pointerToMappedRegion + sizeof(uint64_t) * 2, // 2
        (char*)pointerToMappedRegion + sizeof(uint64_t) * 3 + sizeof(link_index) * 0, // 3
        (char*)pointerToMappedRegion + sizeof(uint64_t) * 3 + sizeof(link_index) * 1, // 4
        (char*)pointerToMappedRegion + sizeof(uint64_t) * 3 + sizeof(link_index) * 2, // 5

        // Links Block
        (char*)pointerToMappedRegion + serviceBlockSizeInBytes // 6
    };

    pointerToDataSeal = (uint64_t*)pointers[0];
    pointerToLinkIndexSize = (uint64_t*)pointers[1];
    pointerToMappingLinksMaxSize = (uint64_t*)pointers[2];
    pointerToLinksMaxSize = (link_index*)pointers[3];
    pointerToLinksSize = (link_index*)pointers[4];
    pointerToPointerToMappingLinks = (link_index*)pointers[5];

    pointerToLinks = (Link*)pointers[6];
    pointerToUnusedMarker = pointerToLinks;

#ifdef DEBUG
    printf("DataSeal            = %" PRIu64 "\n", *pointerToDataSeal);
    printf("LinkIndexSize       = %" PRIu64 "\n", *pointerToLinkIndexSize);
    printf("MappingLinksMaxSize = %" PRIu64 "\n", *pointerToMappingLinksMaxSize);
    printf("LinksMaxSize        = %" PRIu64 "\n", (uint64_t)*pointerToLinksMaxSize);
    printf("LinksSize           = %" PRIu64 "\n", (uint64_t)*pointerToLinksSize);
#endif

    uint64_t expectedMappingLinksMaxSize = baseLinksSizeInBytes / sizeof(link_index);

    if (*pointerToDataSeal == LINKS_DATA_SEAL_64BIT)
    { // opening
        DebugInfo("Storage file opened.");

        if (*pointerToLinkIndexSize != sizeof(link_index))
            return ResetStorageFileMapping() & Error("Opening storage file with different link index size is not supported yet.") & CloseStorageFile();

        if (*pointerToMappingLinksMaxSize != expectedMappingLinksMaxSize)
            return ResetStorageFileMapping() & Error("Opening storage file with different system page size is not supported yet.") & CloseStorageFile();

        if (*pointerToLinksSize > *pointerToLinksMaxSize)
            return ResetStorageFileMapping() & Error("Saved links size counter is set to bigger value than maximum allowed size. Storage file is damaged.") & CloseStorageFile();

        *pointerToLinksMaxSize = (storageFileSizeInBytes - serviceBlockSizeInBytes) / sizeof(Link);

        // TODO: Varidate all mapped links are exist (otherwise reset them to 0) (fast)
        // TODO: Varidate all freed link (holes). (slower)
        // TODO: Varidate all links. (slowest)
    }
    else
    { // creation
        DebugInfo("Storage file created.");

        *pointerToLinkIndexSize = sizeof(link_index);
        *pointerToMappingLinksMaxSize = expectedMappingLinksMaxSize;
        *pointerToLinksMaxSize = (storageFileSizeInBytes - serviceBlockSizeInBytes) / sizeof(Link);
        *pointerToLinksSize = 1; // null element (unused link marker) always exists

        // Only if mmap does not put zeros
        if (*pointerToPointerToMappingLinks)
            memset(pointerToPointerToMappingLinks, 0, baseLinksSizeInBytes);
        if (!IsNullLinkEmpty())
            memset(pointerToLinks, 0, sizeof(Link));
    }

    DebugInfo("Memory mapping of storage file is set.");

    PrintLinksDatabaseSize();

    return SUCCESS_RESULT;
}

signed_integer EnlargeStorageFile()
{
    if (succeeded(EnsureStorageFileOpened()))
    {
        if (succeeded(EnsureStorageFileMapped()))
        {
            if (succeeded(ResetStorageFileMemoryMapping()))
            {
                if (storageFileSizeInBytes >= storageFileMinSizeInBytes)
                    storageFileSizeInBytes += baseBlockSizeInBytes;
                else
                    return Error("File size is less than minimum allowed size.");

                if (succeeded(SetStorageFileMemoryMapping()))
                    return SUCCESS_RESULT;
            }
        }
    }

    return ERROR_RESULT;
}

signed_integer ShrinkStorageFile()
{
    if (succeeded(EnsureStorageFileOpened()))
    {
        if (succeeded(EnsureStorageFileMapped()))
        {
            if (storageFileSizeInBytes > storageFileMinSizeInBytes)
            {
                link_index linksTableNewMaxSize = (storageFileSizeInBytes - serviceBlockSizeInBytes - baseBlockSizeInBytes) / sizeof(Link);

                if (*pointerToLinksSize < linksTableNewMaxSize)
                {
                    if (succeeded(ResetStorageFileMemoryMapping()))
                    {
                        storageFileSizeInBytes -= baseBlockSizeInBytes;

                        if (succeeded(SetStorageFileMemoryMapping()))
                            return SUCCESS_RESULT;
                    }
                }
            }
        }
    }

    return ERROR_RESULT;
}

signed_integer ResetStorageFileMemoryMapping()
{
    if (succeeded(EnsureStorageFileOpened()))
    {
        if (succeeded(EnsureStorageFileMapped()))
        {
            DebugInfo("Resetting memory mapping of storage file...");

            PrintLinksDatabaseSize();

            if (*pointerToDataSeal != LINKS_DATA_SEAL_64BIT)
            {
                *pointerToDataSeal = LINKS_DATA_SEAL_64BIT; // Запечатываем файл
                DebugInfo("Storage file sealed.");
            }

            // Считаем реальный размер файла
            int64_t lastFileSizeInBytes = *pointerToDataSeal == LINKS_DATA_SEAL_64BIT ? (int64_t)(serviceBlockSizeInBytes + *pointerToLinksSize * sizeof(Link)) : storageFileSizeInBytes;

#if defined(WINDOWS)
            UnmapViewOfFile(pointerToMappedRegion);
            CloseHandle(storageFileMappingHandle);
#elif defined(LINUX)
            munmap(pointerToMappedRegion, storageFileSizeInBytes);
#endif

            ResetStorageFileMapping();

            // Обновляем текущий размер файла в соответствии с реальным (чтобы при закрытии файла сделать его размер минимальным).
            storageFileSizeInBytes = lastFileSizeInBytes;

            DebugInfo("Memory mapping of storage file is reset.");

            return SUCCESS_RESULT;
        }
    }

    return ERROR_RESULT;
}

signed_integer CloseStorageFile()
{
    if (succeeded(EnsureStorageFileOpened()))
    {
        if (succeeded(EnsureStorageFileUnmapped()))
        {
            DebugInfo("Closing storage file...");

            // Перед закрытием файла обновляем его размер файла (это гарантирует его минимальный размер).
            ResizeStorageFile();

#if defined(WINDOWS)
            if (storageFileHandle == INVALID_HANDLE_VALUE) // т.к. например STDIN_FILENO == 0 - для stdin (под Linux)
                // Убран принудительный выход, так как даже в случае неправильного дескриптора, его можно попытаться закрыть
                DebugInfo("Storage file is not open or already closed. Let's try to close it anyway.");

            CloseHandle(storageFileHandle);
#elif defined(LINUX)
            if (storageFileHandle == -1)
                return Error("Storage file is not open or already closed.");

            close(storageFileHandle);
#endif

            ResetStorageFile();

            DebugInfo("Storage file closed.");

            return SUCCESS_RESULT;
        }
    }

    return ERROR_RESULT;
}

signed_integer OpenLinks(char* filename){
    InitPersistentMemoryManager();
    signed_integer result = OpenStorageFile("db.links");
    if (!succeeded(result))
        return result;
    return SetStorageFileMemoryMapping();
}

signed_integer CloseLinks(){
    signed_integer result = ResetStorageFileMemoryMapping();
    if (!succeeded(result))
        return result;
    return CloseStorageFile();
}

link_index AllocateFromUnusedLinks()
{
    link_index unusedLinkIndex = pointerToUnusedMarker->ByLinkerRootIndex;
    DetachLinkFromUnusedMarker(unusedLinkIndex);
    return unusedLinkIndex;
}

link_index AllocateFromFreeLinks()
{
    if (*pointerToLinksMaxSize == *pointerToLinksSize)
        EnlargeStorageFile();

    return (*pointerToLinksSize)++;
}

link_index AllocateLink()
{
    if (pointerToUnusedMarker->ByLinkerRootIndex != null)
        return AllocateFromUnusedLinks();
    else
        return AllocateFromFreeLinks();
    return null;
}

void FreeLink(link_index linkIndex)
{
    Link *link = GetLink(linkIndex);
    Link* lastUsedLink = pointerToLinks + *pointerToLinksSize - 1;

    if (link < lastUsedLink)
    {
        AttachLinkToUnusedMarker(linkIndex);
    }
    else if (link == lastUsedLink)
    {
        --*pointerToLinksSize;

        while ((--lastUsedLink)->LinkerIndex == null && pointerToLinks != lastUsedLink) // Не существует и не является 0-й связью
        {
            DetachLinkFromUnusedMarker(GetLinkIndex(lastUsedLink));
            --*pointerToLinksSize;
        }

        ShrinkStorageFile(); // Размер будет уменьшен, только если допустимо
    }
}

void WalkThroughAllLinks(visitor visitor)
{
    if (*pointerToLinksSize <= 1)
        return;

    Link* currentLink = pointerToLinks + 1;
    Link* lastLink = pointerToLinks + *pointerToLinksSize - 1;

    do {
        if (ExistsLink(currentLink)) visitor(GetLinkIndex(currentLink));
    } while (++currentLink <= lastLink);
}

signed_integer WalkThroughLinks(stoppable_visitor stoppableVisitor)
{
    if (*pointerToLinksSize <= 1)
        return true;

    Link* currentLink = pointerToLinks + 1;
    Link* lastLink = pointerToLinks + *pointerToLinksSize - 1;

    do {
        if (ExistsLink(currentLink) && !stoppableVisitor(GetLinkIndex(currentLink))) return false;
    } while (++currentLink <= lastLink);

    return true;
}

link_index GetMappedLink(signed_integer mappingIndex)
{
    if (mappingIndex >= 0 && mappingIndex < (signed_integer)*pointerToMappingLinksMaxSize)
        return pointerToPointerToMappingLinks[mappingIndex];
    else
        return null;
}

void SetMappedLink(signed_integer mappingIndex, link_index linkIndex)
{
    if (mappingIndex >= 0 && mappingIndex < (signed_integer)*pointerToMappingLinksMaxSize)
        pointerToPointerToMappingLinks[mappingIndex] = linkIndex;
}
