#include <stdio.h> // printf
#define __USE_XOPEN_EXTENDED
#include <stdlib.h> // random
#include <time.h> // time

#include "sbt.h"

int main() {

	// третий пример
	// генерация псевдослучайных чисел
#define N 120
#define RND_SEED 100
#define RND_A 9
#define RND_B 9
#define RND_C 7
	int rnd = RND_SEED;
	for(int i = 0; i < N; i++) {
		rnd ^= (rnd << RND_A);
		rnd ^= (rnd >> RND_B);
		rnd ^= (rnd << RND_C);
		//SBT_AddNodeUniq((rnd)&0x000000FF); // вставка с отказами
		SBT_AddNode((rnd)&0x000000FF); // вставка без отказов
	}

	// результат работы
	SBT_CheckAllNodes();
//	SBT_PrintAllNodes();
//	SBT_DumpAllNodes();
	//return 0;

	printf("удаление ...\n");

	rnd = RND_SEED + 1;
	for(int i = 0; i < N; i++) {
		rnd ^= (rnd << RND_A);
		rnd ^= (rnd >> RND_B);
		rnd ^= (rnd << RND_C);
//		SBT_PrintAllNodes();
		printf("удаление idx = %d\n", (rnd)&0x000000FF);
		SBT_DeleteNode((rnd)&0x000000FF); // вставка без отказов
//		SBT_CheckAllNodesSize();
//		SBT_CheckAllNodes();
		SBT_CheckAllNodesBalance();
	}
	SBT_PrintAllNodes();
//	SBT_CheckAllNodesSize();
	SBT_CheckAllNodes();

	return 0;
}
