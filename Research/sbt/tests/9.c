#include <stdio.h> // printf
#define __USE_XOPEN_EXTENDED
#include <stdlib.h> // random
#include <time.h> // time

#include "sbt.h"

int main() {


	// девятый пример

/*
	SBT_AddNode(1);

	// распечатать результат
	SBT_CheckAllNodes();
	SBT_PrintAllNodes();
	SBT_DumpAllNodes();

	SBT_DeleteNode(1);

	// распечатать результат
	SBT_CheckAllNodes();
	SBT_PrintAllNodes();
	SBT_DumpAllNodes();
*/


/*
	SBT_AddNode(1);
	SBT_AddNode(2);
	SBT_AddNode(3);
	SBT_AddNode(4);
	SBT_AddNode(5);
	SBT_AddNode(6);
	SBT_AddNode(7);

	SBT_DeleteNode(4);

	// распечатать результат
	SBT_CheckAllNodes();
	SBT_PrintAllNodes();
	SBT_DumpAllNodes();
*/

/*
//	for(int i = 0; i < 2; i++) SBT_AddNode(i);
	SBT_AddNode(5);
	SBT_AddNode(4);
	SBT_CheckAllNodes(); SBT_PrintAllNodes(); SBT_DumpAllNodes();
	SBT_DeleteNode(4);
	SBT_CheckAllNodes(); SBT_PrintAllNodes(); SBT_DumpAllNodes();
*/

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

	SBT_DeleteNode(11);

	SBT_CheckAllNodes(); SBT_PrintAllNodes(); SBT_DumpAllNodes();

	return 0;
}
