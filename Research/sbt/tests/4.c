#include <stdio.h> // printf
#define __USE_XOPEN_EXTENDED
#include <stdlib.h> // random
#include <time.h> // time

#include "sbt.h"

int onRotate(TNodeIndex nodeIndex1, TNodeIndex nodeIndex2, const char *stringAction) {
        printf("%lld %lld %s\n",
                (long long int)nodeIndex1,
                (long long int)nodeIndex2,
                stringAction
        );
        return 0;
}

int main() {

	SBT_SetCallback_OnRotate(onRotate);

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
