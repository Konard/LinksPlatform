#include "Link.h"

int main() {
	InitPersistentMemoryManager();
	OpenStorageFile("test.bin");
	SetStorageFileMemoryMapping();

//#define itself ((Link *)0)

uint64_t isA = CreateLink(itself, itself, itself);
uint64_t isNotA = CreateLink(itself, itself, isA);
uint64_t link = CreateLink(itself, isA, itself);
uint64_t thing = CreateLink(itself, isNotA, link);

UpdateLink(isA, isA, isA, link); // После этого минимальное ядро системы можно считать сформированным

DeleteLink(isA); // Одна эта операция удалит все 4 связи

	CloseStorageFile();
	return 0;
}
