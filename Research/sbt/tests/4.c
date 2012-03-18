#include <stdio.h> // printf
#define __USE_XOPEN_EXTENDED
#include <stdlib.h> // random
#include <time.h> // time

#include "sbt.h"

int onRotate(TNodeIndex nodeIndex1, TNodeIndex nodeIndex2, const char *stringAction) {
        printf("idx1 = %lld, idx2 = %lld, action = %s\n",
                (long long int)nodeIndex1,
                (long long int)nodeIndex2,
                stringAction
        );
        return 0;
}

int main() {

	SBT_SetCallback_OnRotate(onRotate);

	// четвертый пример
	// добавить 5 вершин
//	SBT_AddNode(15);
//	SBT_AddNode(25);
//	SBT_AddNode(20);
//	SBT_AddNode(22);
//	SBT_AddNode(19);

	// генерация псевдослучайных чисел
#define RND_SEED 100
#define RND_A 9
#define RND_B 9
#define RND_C 7
	int rnd = RND_SEED;
	for(int i = 0; i < 10000000; i++) {
		rnd ^= (rnd << RND_A);
		rnd ^= (rnd >> RND_B);
		rnd ^= (rnd << RND_C);
		//SBT_AddNodeUniq((rnd)&0x000000FF); // вставка с отказами
		SBT_AddNode((rnd)&0x000000FF); // вставка без отказов
	}

	// результат работы
//	SBT_CheckAllNodes();
//	SBT_PrintAllNodes();
//	SBT_DumpAllNodes();

	return 0;
}
