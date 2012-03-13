#include <stdio.h> // printf
#define __USE_XOPEN_EXTENDED
#include <stdlib.h> // random
#include <time.h> // time

#include "sbt.h"

int main() {

	// второй пример
	SBT_Add(1);
	SBT_Add(2);
	SBT_Add(3);
	SBT_Add(4);
	SBT_Add(5);
	SBT_Add(6);
	SBT_Add(7);
	SBT_Add(8);
	SBT_Add(9);
	SBT_Add(10);
	SBT_Add(11);
	SBT_Add(12);

	SBT_Add(5);
	SBT_Add(6);
	SBT_Add(7);
	SBT_CheckAllNodes();
	SBT_PrintAllNodes();
	SBT_DumpAllNodes();

	return 0;
}
