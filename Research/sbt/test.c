#include <stdio.h> // printf
#define __USE_XOPEN_EXTENDED
#include <stdlib.h> // random
#include <time.h> // time

#include "sbt.h"

int main() {
/*
	srandom(time(NULL)%1000);
	for(int i = 0; i < 10; i++) {
		SBT_Add(random() % 1000);
		SBT_PrintAllNodes();
	}
*/

	// второй пример
	SBT_Add(1);
	SBT_Add(2);
	SBT_Add(3);
	SBT_Add(4);
	SBT_Add(5);

	return 0;
}
