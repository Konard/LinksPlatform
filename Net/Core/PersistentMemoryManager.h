__declspec(dllexport) void InitPersistentMemoryManager();
__declspec(dllexport) unsigned long OpenStorageFile(char *filename);
__declspec(dllexport) unsigned long CloseStorageFile();
__declspec(dllexport) unsigned long EnlargeStorageFile();
__declspec(dllexport) unsigned long ShrinkStorageFile();
__declspec(dllexport) unsigned long SetStorageFileMemoryMapping();
__declspec(dllexport) unsigned long ResetStorageFileMemoryMapping();
__declspec(dllexport) Link* GetMappedLink(int index);
__declspec(dllexport) void SetMappedLink(int index, Link* link);
__declspec(dllexport) void ReadTest();
__declspec(dllexport) void WriteTest();

Link* AllocateLink();
void FreeLink(Link* link);

__declspec(dllexport) void WalkThroughAllLinks(void __stdcall func(Link *));
__declspec(dllexport) int WalkThroughLinks(int __stdcall func(Link *));