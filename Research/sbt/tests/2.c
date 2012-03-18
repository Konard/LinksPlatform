#include <stdio.h> // printf
#define __USE_XOPEN_EXTENDED
#include <stdlib.h> // random
#include <time.h> // time

#include "sbt.h"

int main() {

	// второй пример
	// добавить 12 вершин (они будут передвигаться налево,
	// так как подвешиваются справа - идут по-возрастанию)
	SBT_AddNode(1);
	SBT_AddNode(2);
	SBT_AddNode(3);
	SBT_AddNode(4);
	SBT_AddNode(5);
	SBT_AddNode(6);
	SBT_AddNode(7);
	SBT_AddNode(8);
	SBT_AddNode(9);
	SBT_AddNode(10);
	SBT_AddNode(11);
	SBT_AddNode(12);

	// посреди них ещё три вершины
	SBT_AddNode(5);
	SBT_AddNode(6);
	SBT_AddNode(7);
	SBT_DeleteNode(6);

	// распечатать результат
	SBT_CheckAllNodes();
	SBT_PrintAllNodes();
	SBT_DumpAllNodes();

	return 0;
}
