#include <stdio.h> // printf
#define __USE_XOPEN_EXTENDED
#include <stdlib.h> // random
#include <time.h> // time

#include "sbt.h"

int main() {

//#define N 10000000
#define N 1000
	// седьмой пример
	printf("добавление ...\n");
	for (int i = 0; i < N; i++) {
		SBT_AddNodeUniq(i);
//		SBT_CheckAllNodesSize();
	}

	printf("удаление ...\n");
	for (int i = 0; i < N; i++) {
//	printf("перед удалением %d\n", i);
//	SBT_DumpAllNodes();
//	SBT_PrintAllNodes();
		SBT_DeleteNode(i);
		printf("после удаления %d\n", i);
		SBT_CheckAllNodesBalance();
//	SBT_DumpAllNodes();
//	SBT_PrintAllNodes();
//		SBT_CheckAllNodesSize();
	}
//	SBT_CheckAllNodesSize();
//	SBT_CheckAllNodesBalance();

//	    SBT_DeleteAllNodes(i);
//	SBT_DumpAllNodes();
//	SBT_PrintAllNodes();

	return 0;
}
