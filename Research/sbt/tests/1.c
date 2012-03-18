#include <stdio.h> // printf
#define __USE_XOPEN_EXTENDED
#include <stdlib.h> // random
#include <time.h> // time

#include "sbt.h"

int main() {

	// первый пример
	srandom(time(NULL)%1000); // запустить генератор случайных чисел по таймеру программы

	// проверить работу библиотеки на примере постепенного добавления в дерево случайных значений
	for(int i = 0; i < 5; i++) {
		SBT_AddNode(random() % 1000);
		SBT_DumpAllNodes();
		SBT_PrintAllNodes();
	}
//	SBT_CheckAllNodes();
//	SBT_PrintAllNodes();
//	SBT_DumpAllNodes();

	return 0;
}
