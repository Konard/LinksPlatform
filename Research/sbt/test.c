#include <stdio.h> // printf
#define __USE_XOPEN_EXTENDED
#include <stdlib.h> // random
#include <time.h> // time

#include "sbt.h"

int main() {
	srandom(time(NULL)%1000);
	for(int i = 0; i < 100; i++) {
		SBT_Add(random() % 1000);
	}
	SBT_PrintAllNodes();
	return 0;
}
