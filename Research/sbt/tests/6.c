#include <stdio.h> // printf
#define __USE_XOPEN_EXTENDED
#include <stdlib.h> // random
#include <time.h> // time

#include "sbt.h"

int main() {

	// шестой пример
	TNumber idx = SBT_FindNode(2); // = обычный Find для уникального ключа (number)
	printf("idx(2) = %lld\n", (long long int)idx);

	SBT_AddNodeUniq(2);
	SBT_AddNodeUniq(1);
	SBT_AddNodeUniq(2); // fail
	SBT_AddNodeUniq(1); // fail
	SBT_CheckAllNodes();

	printf("в начале:\n");
	SBT_PrintAllNodes();
	SBT_DumpAllNodes();
	SBT_CheckAllNodes();

	printf("delete idx(1) = %d\n", SBT_DeleteNode(1));
	printf("после удаления единицы:\n");
	SBT_PrintAllNodes();
	SBT_DumpAllNodes();
	SBT_CheckAllNodes();

	printf("delete idx(2) = %d\n", SBT_DeleteNode(2));
	printf("после удаления двойки:\n");
	SBT_PrintAllNodes();
	SBT_DumpAllNodes();
	SBT_CheckAllNodes();

	return 0;
}
