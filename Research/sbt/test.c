#include <stdio.h> // printf
#define __USE_XOPEN_EXTENDED
#include <stdlib.h> // random
#include <time.h> // time

#include "sbt.h"

int main() {

/*
	// первый пример
	srandom(time(NULL)%1000);
	for(int i = 0; i < 50; i++) {
		SBT_Add(random() % 1000);
		SBT_PrintAllNodes();
	}
*/

/*
	// второй пример
	SBT_Add(1);
	SBT_Add(2);
	SBT_Add(3);
	SBT_Add(4);
	SBT_Add(5);
	SBT_Add(6);
	SBT_Add(7);
	SBT_Add(8);
	SBT_Add(9);
	SBT_Add(10);
	SBT_Add(11);
	SBT_Add(12);

	SBT_Add(5);
	SBT_Add(6);
	SBT_Add(7);
	SBT_CheckAllNodes();
	SBT_PrintAllNodes();
	SBT_DumpAllNodes();
*/

	// третий пример
#define RND_SEED 100
#define RND_A 9
#define RND_B 9
#define RND_C 7
	int rnd = RND_SEED;
	for(int i = 0; i < 1000000; i++) {
		rnd ^= (rnd << RND_A);
		rnd ^= (rnd >> RND_B);
		rnd ^= (rnd << RND_C);
		SBT_AddUniq((rnd)&0x000000FF);
//		SBT_PrintAllNodes();
	}
	SBT_CheckAllNodes();
//	SBT_PrintAllNodes();
	SBT_DumpAllNodes();

/*
	// четвертый пример
	SBT_Add(15);
	SBT_Add(25);
	SBT_Add(20);
	SBT_Add(22);
	SBT_Add(19);
	SBT_CheckAllNodes();
	SBT_PrintAllNodes();
	SBT_DumpAllNodes();
*/

/*
	// пятый пример
	for (int i = 0; i < 10; i++)
	    SBT_AddUniq(i*2);
	for (int i = 0; i < 10; i++)
	    SBT_AddUniq(i*2 + 1);
	for (int i = 0; i < 10; i++)
	    SBT_AddUniq(i*2 + 1); // fail

	SBT_PrintAllNodes();
	SBT_CheckAllNodes();
	
	TNumber n = SBT_FindFirstNode(2); // = обычный Find для уникального ключа (number)
	printf("%lld\n", n);
	
	for (int i = 0; i < 100; i++)
	    SBT_Delete(i);
	SBT_CheckAllNodes();
*/

/*
	// шестой пример
//	TNumber n = SBT_FindFirstNode(2); // = обычный Find для уникального ключа (number)
//	printf("n = %lld\n", n);
	SBT_AddUniq(2);
	SBT_AddUniq(1);
	SBT_AddUniq(2);
	SBT_AddUniq(1);
	SBT_DumpAllNodes();
	SBT_PrintAllNodes();
	SBT_CheckAllNodes();
*/

	return 0;
}
