#include <stdio.h> // printf
#define __USE_XOPEN_EXTENDED
#include <stdlib.h> // random
#include <time.h> // time

#include "sbt.h"

int main() {

#define N 10000
	// седьмой пример
	for (int i = 0; i < N; i++) {
		SBT_AddNodeUniq(i);
		SBT_CheckAllNodesSize();
	}
	for (int i = 0; i < N; i++) {
		SBT_DeleteNode(i);
		SBT_CheckAllNodesSize();
	}

//	    SBT_DeleteAllNodes(i);
//	SBT_DumpAllNodes();
//	SBT_PrintAllNodes();

	return 0;
}
