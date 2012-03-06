#include <stdio.h> // printf
#define __USE_XOPEN_EXTENDED
#include <stdlib.h> // random
#include <time.h> // time

#include "sbt.h"

int main() {

/*
	srandom(time(NULL)%1000);
	for(int i = 0; i < 50; i++) {
		SBT_Add(random() % 1000);
		SBT_PrintAllNodes();
	}
*/
#define RND_SEED 100
#define RND_A 9
#define RND_B 9
#define RND_C 7
	int rnd = RND_SEED;
	for(int i = 0; i < 50; i++) {
		rnd ^= (rnd << RND_A);
		rnd ^= (rnd >> RND_B);
		rnd ^= (rnd << RND_C);
		SBT_Add((rnd)&0x000000FF);
		SBT_PrintAllNodes();
	}
	SBT_CheckAllNodes();

/*
	// второй пример
	SBT_Add(1);
	SBT_Add(2);
	SBT_Add(3);
	SBT_Add(4);
	SBT_Add(5);
*/
	return 0;
}
