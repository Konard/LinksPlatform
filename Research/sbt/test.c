#include <stdio.h>

#include "sbt.h"

int main() {
	for(int i = 0; i < 100; i++) {
		SBT_Add(i);
	}
	SBT_PrintAllNodes();
	return 0;
}
