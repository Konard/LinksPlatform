#include <stdio.h> // printf
#define __USE_XOPEN_EXTENDED
#include <stdlib.h> // random
#include <time.h> // time

#include "sbt.h"

int main() {

	// четвертый пример
	SBT_Add(15);
	SBT_Add(25);
	SBT_Add(20);
	SBT_Add(22);
	SBT_Add(19);
	SBT_CheckAllNodes();
	SBT_PrintAllNodes();
	SBT_DumpAllNodes();
	SBT_CheckAllNodes();
	SBT_PrintAllNodes();
	SBT_DumpAllNodes();

	return 0;
}
