
#include "sbt.h"

#include <pthread.h> // pthread_mutex_*
// узкое место при доступе из многих потоков - глобальная блокировка
pthread_mutex_t _lock_nodes = PTHREAD_MUTEX_INITIALIZER;
TNodeIndex _root_index = -1;
TNode _nodes[SBT_MAX_NODES];

int SBT_Add(TNumber n) {
	return 0;
}

int SBT_Delete(TNumber n) {
	return 0;
}

void SBT_PrintAllNodes_At(TNodeIndex t) {
	
}

void SBT_PrintAllNodes() {
	SBT_PrintAllNodes_At(_root_index);
}
