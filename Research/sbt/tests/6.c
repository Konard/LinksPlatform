#include <stdio.h> // printf
#define __USE_XOPEN_EXTENDED
#include <stdlib.h> // random
#include <time.h> // time

#include "sbt.h"

int main() {

	// шестой пример
//	TNumber n = SBT_FindFirstNode(2); // = обычный Find для уникального ключа (number)
//	printf("n = %lld\n", n);
	SBT_AddNodeUniq(2);
	SBT_AddNodeUniq(1);
	SBT_AddNodeUniq(2);
	SBT_AddNodeUniq(1);
	SBT_CheckAllNodes();
	printf("в начале\n");
	SBT_PrintAllNodes();

	printf("delete 1: %d\n", SBT_DeleteNode(1));
//	printf("root index = %lld\n", GetRootIndex());
	printf("после удаления единицы:\n");
	SBT_PrintAllNodes();
//	SBT_DumpAllNodes();

	printf("delete 2: %d\n", SBT_DeleteNode(2));
//	printf("root index = %lld\n", GetRootIndex());
	printf("после удаления двойки:\n");
	SBT_PrintAllNodes();

//	SBT_DumpAllNodes();


	return 0;
}
