#include <stdio.h> // printf
#define __USE_XOPEN_EXTENDED
#include <stdlib.h> // random
#include <time.h> // time

#include "sbt.h"

int main() {

	// шестой пример
//	TNumber n = SBT_FindFirstNode(2); // = обычный Find для уникального ключа (number)
//	printf("n = %lld\n", n);
	SBT_AddUniq(2);
	SBT_AddUniq(1);
	SBT_AddUniq(2);
	SBT_AddUniq(1);
	SBT_CheckAllNodes();
	printf("в начале\n");
	SBT_PrintAllNodes();

	printf("delete 1: %d\n", SBT_Delete(1));
//	printf("root index = %lld\n", GetRootIndex());
	printf("после удаления единицы:\n");
	SBT_PrintAllNodes();
//	SBT_DumpAllNodes();

	printf("delete 2: %d\n", SBT_Delete(2));
//	printf("root index = %lld\n", GetRootIndex());
	printf("после удаления двойки:\n");
	SBT_PrintAllNodes();

//	SBT_DumpAllNodes();


	return 0;
}
