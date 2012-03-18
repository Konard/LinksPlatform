#include <stdio.h> // printf
#define __USE_XOPEN_EXTENDED
#include <stdlib.h> // random
#include <time.h> // time

#include "sbt.h"

int main() {

	// седьмой пример
	for (int i = 0; i < 10000000; i++)
	    SBT_AddNodeUniq(i);
	for (int i = 0; i < 10000000; i++)
	    SBT_DeleteAll(i);
//	SBT_DumpAllNodes();
//	SBT_PrintAllNodes();

	return 0;
}
