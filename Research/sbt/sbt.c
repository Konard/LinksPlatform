
#include "sbt.h"

#include <pthread.h> // pthread_mutex_*
// узкое место при доступе из многих потоков - глобальная блокировка
pthread_mutex_t _lock_nodes = PTHREAD_MUTEX_INITIALIZER;
TNode _nodes[SBT_MAX_NODES];

int SBT_Add(TNumber n) {
}

void SBT_PrintAllNodes() {
}
