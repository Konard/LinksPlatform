#include <stdio.h> // printf
#define __USE_XOPEN_EXTENDED
#include <stdlib.h> // random
#include <time.h> // time

#include "sbt.h"

int main() {

	// четвертый пример
	SBT_AddNode(15);
	SBT_AddNode(25);
	SBT_AddNode(20);
	SBT_AddNode(22);
	SBT_AddNode(19);
	SBT_CheckAllNodes();
	SBT_PrintAllNodes();
	SBT_DumpAllNodes();

	return 0;
}
