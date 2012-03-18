#include <stdio.h> // printf
#define __USE_XOPEN_EXTENDED
#include <stdlib.h> // random
#include <time.h> // time

#include "sbt.h"

int main() {

	// пятый пример
	for (int i = 0; i < 10; i++)
	    SBT_AddNodeUniq(i*2);
	for (int i = 0; i < 10; i++)
	    SBT_AddNodeUniq(i*2 + 1);
	for (int i = 0; i < 10; i++)
	    SBT_AddNodeUniq(i*2 + 1); // fail

	SBT_CheckAllNodes();
	SBT_PrintAllNodes();
	
	TNumber idx = SBT_FindFirstNode(2); // = обычный Find для уникального ключа (number)
	printf("idx(2) = %lld\n", (long long int)idx);
	
	for (int i = 0; i < 100; i++)
	    SBT_DeleteNode(i);

	SBT_CheckAllNodes();
//	SBT_PrintAllNodes();
	SBT_DumpAllNodes();

	return 0;
}
