
#include "sbt.h"

#include <stdio.h> // printf

#include <pthread.h> // pthread_mutex_*
// узкое место при доступе из многих потоков - глобальная блокировка таблицы _nodes
pthread_mutex_t _lock_nodes = PTHREAD_MUTEX_INITIALIZER;
TNodeIndex _root_index = -1;
TNode _nodes[SBT_MAX_NODES];
TNodeIndex _n_nodes = 0;

int SBT_Add_At(TNumber number, TNodeIndex t) {
	_nodes[_n_nodes].number = number;
	if (_n_nodes <= 0) {
		_nodes[_n_nodes].left = -1;
		_nodes[_n_nodes].right = -1;
		_nodes[_n_nodes].size = 1;
		_root_index = 0;
		_n_nodes++;
	}
	else {
		if(number < _nodes[t].number) {
			if(_nodes[t].left == -1) {
				_nodes[t].left = _n_nodes;
				_nodes[_n_nodes].number = number;
				_nodes[_n_nodes].left = -1;
				_nodes[_n_nodes].right = -1;
				_nodes[_n_nodes].size = 1;
				_n_nodes++;
			}
			else {
				SBT_Add_At(number, _nodes[t].left);
			}
		}
		else {
			if(_nodes[t].right == -1) {
				_nodes[t].right = _n_nodes;
				_nodes[_n_nodes].number = number;
				_nodes[_n_nodes].left = -1;
				_nodes[_n_nodes].right = -1;
				_nodes[_n_nodes].size = 1;
				_n_nodes++;
			}
			else {
				SBT_Add_At(number, _nodes[t].right);
			}
		}
	}
	return 0;
}

int SBT_Add(TNumber number) {
	return SBT_Add_At(number, _root_index);
}

int SBT_Delete(TNumber n) {
	return 0;
}

void SBT_PrintAllNodes_At(int depth, TNodeIndex t) {
	if ((_n_nodes <= 0) || (t < 0)) return; // выйти, если вершины нет
	if (_nodes[t].left > 0) SBT_PrintAllNodes_At(depth + 1, _nodes[t].left);
	printf("depth = %d, node = "SBT_FORMAT_STRING": ("SBT_FORMAT_STRING")\n", depth, t, (SBT_FORMAT_TYPE)_nodes[t].number); // иначе: напечатать "тело" узла
	if (_nodes[t].right > 0) SBT_PrintAllNodes_At(depth + 1, _nodes[t].right);
}

void SBT_PrintAllNodes() {
	SBT_PrintAllNodes_At(0, _root_index);
	printf("_n_nodes = "SBT_FORMAT_STRING"\n", _n_nodes);
}
