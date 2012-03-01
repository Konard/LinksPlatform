
#include "sbt.h"

#include <stdio.h> // printf

#include <pthread.h> // pthread_mutex_*
// узкое место при доступе из многих потоков - глобальная блокировка
pthread_mutex_t _lock_nodes = PTHREAD_MUTEX_INITIALIZER;
TNodeIndex _root_index = -1;
TNode _nodes[SBT_MAX_NODES];
TNodeIndex _n_nodes = 0;

int SBT_Add(TNumber number) {
	_nodes[_n_nodes].number = number;
	if (_n_nodes > 0) {
		_nodes[_n_nodes-1].left = -1;
		_nodes[_n_nodes-1].right = _n_nodes;
		_nodes[_n_nodes].left = -1;
		_nodes[_n_nodes].right = -1;
	}
	else {
		_nodes[_n_nodes].left = -1;
		_nodes[_n_nodes].right = -1;
		_root_index = 0;
	}
	_n_nodes++;
	return 0;
}

int SBT_Delete(TNumber n) {
	return 0;
}

void SBT_PrintAllNodes_At(int depth, TNodeIndex t) {
	if ((_n_nodes <= 0) || (t < 0)) return; // выйти, если вершины нет
	printf("%d "SBT_FORMAT_STRING" "SBT_FORMAT_STRING"\n", depth, t, (SBT_FORMAT_TYPE)_nodes[t].number); // иначе: напечатать "тело" узла
	if (_nodes[t].left > 0) SBT_PrintAllNodes_At(depth + 1, _nodes[t].left);
	if (_nodes[t].right > 0) SBT_PrintAllNodes_At(depth + 1, _nodes[t].right);
}

void SBT_PrintAllNodes() {
	SBT_PrintAllNodes_At(0,_root_index);
}
