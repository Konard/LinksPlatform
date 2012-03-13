#include <stdio.h> // printf
#define __USE_XOPEN_EXTENDED
#include <stdlib.h> // random
#include <time.h> // time

#include "sbt.h"

int main() {

	// первый пример
	srandom(time(NULL)%1000);
	for(int i = 0; i < 50; i++) {
		SBT_Add(random() % 1000);
		SBT_PrintAllNodes();
	}
	SBT_PrintAllNodes();
	SBT_DumpAllNodes();

	return 0;
}
