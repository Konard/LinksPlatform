#include <stdio.h> // printf
#define __USE_XOPEN_EXTENDED
#include <stdlib.h> // random
#include <time.h> // time

#include "sbt.h"

int main() {

	// восьмой пример
	// добавить 6 вершин (они будут передвигаться налево,
	// так как подвешиваются справа - идут по-возрастанию)
	SBT_AddNode(1);
	SBT_AddNode(2);
	SBT_AddNode(3);
	SBT_AddNode(4);
	SBT_AddNode(5);
	SBT_AddNode(6);
	SBT_AddNode(0);
	
	TNodeIndex t;
	t = SBT_FindNode_NearestAndLesser_ByValue(7);
	printf("find < 7: idx = %lld, value = %lld\n", (long long int)t, (long long int)GetValueByIndex(t));
	t = SBT_FindNode_NearestAndLesser_ByValue(4);
	printf("find < 4: idx = %lld, value = %lld\n", (long long int)t, (long long int)GetValueByIndex(t));

	t = SBT_FindNode_NearestAndGreater_ByValue(7);
	printf("find > 7: idx = %lld, value = %lld\n", (long long int)t, (long long int)GetValueByIndex(t));
	t = SBT_FindNode_NearestAndGreater_ByValue(4);
	printf("find > 4: idx = %lld, value = %lld\n", (long long int)t, (long long int)GetValueByIndex(t));

	// распечатать результат
	SBT_CheckAllNodes();
	SBT_PrintAllNodes();
	SBT_DumpAllNodes();

	SBT_DeleteNode(4);

	// распечатать результат
	SBT_CheckAllNodes();
	SBT_PrintAllNodes();
	SBT_DumpAllNodes();

/*
	SBT_DeleteNode(2);

	// распечатать результат
	SBT_CheckAllNodes();
	SBT_PrintAllNodes();
	SBT_DumpAllNodes();
*/


	return 0;
}
