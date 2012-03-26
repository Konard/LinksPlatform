#include <stdio.h> // printf
#define __USE_XOPEN_EXTENDED
#include <stdlib.h> // random, atoi
#include <time.h> // time

#include "sbt.h"

int main(int argc, char **argv) {

	// третий пример
	// генерация псевдослучайных чисел
	if (argc < 2) return 0;
	long long int N = atoll(argv[1]);


#define RND_SEED 100
#define RND_A 9
#define RND_B 9
#define RND_C 7
	int rnd = RND_SEED;
	for(int i = 0; i < N; i++) {
		rnd ^= (rnd << RND_A);
		rnd ^= (rnd >> RND_B);
		rnd ^= (rnd << RND_C);
		SBT_AddNode((rnd)&0x00FFFFFF); // вставка без отказов

		rnd ^= (rnd << RND_A);
		rnd ^= (rnd >> RND_B);
		rnd ^= (rnd << RND_C);
		TNodeIndex t = SBT_FindNextUsedNode(0);

		TNumber v = GetValueByIndex(t);
		printf("t = %lld, v = %lld\n", (long long int)t, (long long int)v);

		SBT_PrintAllNodes();
		if (t != -1)
			SBT_DeleteNode(v); // вставка без отказов
		else 
		printf("[2]\n");
	}

	// результат работы
	SBT_CheckAllNodes();
	SBT_PrintAllNodes();
//	SBT_DumpAllNodes();


/*
{
	SBT_AddNode(123);
	SBT_PrintAllNodes();
	TNodeIndex t = SBT_FindNextUsedNode(0);
	TNumber v = GetValueByIndex(t);
	printf("t = %lld, v = %lld\n", (long long int)t, (long long int)v);
	if (t != -1) SBT_DeleteNode(v); // вставка без отказов
	SBT_PrintAllNodes();

}
printf("---\n");
{
	SBT_PrintAllNodes();
	SBT_AddNode(234);
	SBT_AddNode(234);
	SBT_AddNode(234);
	SBT_PrintAllNodes();
	TNodeIndex t = SBT_FindNextUsedNode(0);
	TNumber v = GetValueByIndex(t);
	printf("t = %lld, v = %lld\n", (long long int)t, (long long int)v);
	if (t != -1) SBT_DeleteNode(v); // вставка без отказов
	SBT_PrintAllNodes();
}
*/

	return 0;
}
