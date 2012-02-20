#include "Link.h"

int main() {
	InitPersistentMemoryManager();
	OpenStorageFile("test.bin");
	SetStorageFileMemoryMapping();
	return 0;
}
