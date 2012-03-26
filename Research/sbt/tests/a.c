#include <stdio.h> // printf
#define __USE_XOPEN_EXTENDED
#include <stdlib.h> // random
#include <time.h> // time

#include "sbt.h"

int main() {


	// десятый пример

	SBT_AddNode(6);
	SBT_AddNode(4);
	SBT_AddNode(7);

	SBT_AddNode(2);
	SBT_AddNode(5);

	SBT_AddNode(11);
	SBT_AddNode(9);
	SBT_AddNode(12);
	SBT_AddNode(8);

	SBT_CheckAllNodes(); SBT_PrintAllNodes(); SBT_DumpAllNodes();

	SBT_DeleteNode(4);
	SBT_DeleteNode(2);

	SBT_DeleteNode(6);
	SBT_DeleteNode(7);
	SBT_DeleteNode(5);
	SBT_DeleteNode(11);
	SBT_DeleteNode(9);
	SBT_DeleteNode(12);
	SBT_DeleteNode(8);
	SBT_DeleteNode(-1);

	SBT_CheckAllNodes(); SBT_PrintAllNodes(); SBT_DumpAllNodes();

	return 0;
}
