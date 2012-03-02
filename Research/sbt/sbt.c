
#include "sbt.h"

#include <stdio.h> // printf

#include <pthread.h> // pthread_mutex_*
// узкое место при доступе из многих потоков - глобальная блокировка таблицы _nodes
pthread_mutex_t _lock_nodes = PTHREAD_MUTEX_INITIALIZER;
TNodeIndex _root_index = -1;
TNode _nodes[SBT_MAX_NODES];
TNodeIndex _n_nodes = 0;

// t - слева, перевешиваем туда
int SBT_LeftRotate(TNodeIndex t) {
	if (t < 0) return 0;
	TNodeIndex k = _nodes[t].right;
	if (k < 0) return 0;

	// поворачиваем ребро дерева
	_nodes[t].right = _nodes[k].left;
	_nodes[k].left = t;

	// корректируем size
	_nodes[k].size = _nodes[t].size;
	TNodeIndex n_l = _nodes[t].left;
	TNodeIndex n_r = _nodes[t].right;
	TNodeSize s_l = ((n_l != -1) ? _nodes[n_l].size : 0);
	TNodeSize s_r = ((n_r != -1) ? _nodes[n_r].size : 0);
	_nodes[t].size = s_l + s_r + 1;

	TNodeIndex p = _nodes[t].parent;
	// меняем трёх предков
	// 1. t.right.parent = t
	// 2. k.parent = t.parent
	// 3. t.parent = k
	if (_nodes[t].right != -1) _nodes[_nodes[t].right].parent = t;
	_nodes[k].parent = _nodes[t].parent;
	_nodes[t].parent = k;

	// меняем корень, parent -> t, k
	printf("  p = %lld (вверх)\n", p);
	if (p == -1) { // это root
		_root_index = k;
	}
	else {
		if (_nodes[p].left == t) {
			_nodes[p].left = k;
		}
		else
		if (_nodes[p].right == t) { // вторую проверку можно не делать
			_nodes[p].right = k;
		}
	}
	return 0;
}

// t - справа, перевешиваем туда
int SBT_RightRotate(TNodeIndex t) {
	if (t < 0) return 0;
	printf("+ rotate t = %lld\n", t);
	printf("  t = %lld (вершина), value = %lld\n", t, _nodes[t].number);
	TNodeIndex k = _nodes[t].left;
	if (k < 0) return 0;
	printf("  k = %lld (левая вершина), value = %lld\n", k, _nodes[k].number);
	printf("(продолжение)\n");

	// поворачиваем ребро дерева
	_nodes[t].left = _nodes[k].right;
	printf("k.right: %lld -> %lld\n", _nodes[k].right, t);
	_nodes[k].right = t;

	// корректируем size
	_nodes[k].size = _nodes[t].size;
	TNodeIndex n_l = _nodes[t].left;
	TNodeIndex n_r = _nodes[t].right;
	TNodeSize s_l = ((n_l != -1) ? _nodes[n_l].size : 0);
	TNodeSize s_r = ((n_r != -1) ? _nodes[n_r].size : 0);
	_nodes[t].size = s_l + s_r + 1;

	TNodeIndex p = _nodes[t].parent;
	// меняем трёх предков
	// 1. t.left.parent = t
	// 2. k.parent = t.parent
	// 3. t.parent = k
	if (_nodes[t].left != -1) _nodes[_nodes[t].left].parent = t;
	_nodes[k].parent = _nodes[t].parent;
	_nodes[t].parent = k;

	// меняем корень, parent -> t, k
	printf("  p = %lld (вверх)\n", p);
	if (p == -1) { // это root
		printf("_root_index: %lld -> %lld\n", _root_index, k);
		_root_index = k;
	}
	else {
		if (_nodes[p].left == t) {
			_nodes[p].left = k;
		}
		else
		if (_nodes[p].right == t) { // вторую проверку можно не делать
			_nodes[p].right = k;
		}
	}
	return 0;
}

int SBT_Maintain(TNodeIndex t, int flag) {
	if ((t >= 0) && (t < _n_nodes)) return 0;
	if (flag == 0) {
		if (1) {
			SBT_RightRotate(t);
		}
		else if (1) {
			SBT_LeftRotate(_nodes[t].left);
			SBT_RightRotate(t);
		}
		else { return 0; }
	}
	else {
		if (1) {
			SBT_LeftRotate(t);
		}
		else if (1) {
			SBT_RightRotate(_nodes[t].right);
			SBT_LeftRotate(t);
		}
		else { return 0; }
	}
	SBT_Maintain(_nodes[t].left, 0); // false
	SBT_Maintain(_nodes[t].right, 1); // true
	SBT_Maintain(t, 0); // false
	SBT_Maintain(t, 1); // true
	return 0;
}

int SBT_Add_At(TNumber number, TNodeIndex t, TNodeIndex parent) {
	_nodes[_n_nodes].number = number;
	if (_n_nodes <= 0) {
		_nodes[_n_nodes].parent = parent;
		_nodes[_n_nodes].left = -1;
		_nodes[_n_nodes].right = -1;
		_nodes[_n_nodes].size = 1;
		_root_index = 0;
		_n_nodes++;
	SBT_PrintAllNodes();
	printf("+ add t = %lld, value = %lld\n", t, number);
	// позже
	// SBT_Maintain(_root_index, (number >= _nodes[t].number) ? 1 : 0);
	//SBT_RightRotate(_root_index);
	SBT_LeftRotate(_root_index);
	}
	else {
		if(number < _nodes[t].number) {
			if(_nodes[t].left == -1) {
				_nodes[t].left = _n_nodes;
				_nodes[_n_nodes].number = number;
				_nodes[_n_nodes].parent = parent;
				_nodes[_n_nodes].left = -1;
				_nodes[_n_nodes].right = -1;
				_nodes[_n_nodes].size = 1;
				_n_nodes++;
	SBT_PrintAllNodes();
	printf("+ add t = %lld, value = %lld\n", t, number);
	// позже
	// SBT_Maintain(_root_index, (number >= _nodes[t].number) ? 1 : 0);
	//SBT_RightRotate(_root_index);
	SBT_LeftRotate(_root_index);
			}
			else {
				SBT_Add_At(number, _nodes[t].left, t);
			}
		}
		else {
			if(_nodes[t].right == -1) {
				_nodes[t].right = _n_nodes;
				_nodes[_n_nodes].number = number;
				_nodes[_n_nodes].parent = parent;
				_nodes[_n_nodes].left = -1;
				_nodes[_n_nodes].right = -1;
				_nodes[_n_nodes].size = 1;
				_n_nodes++;
	SBT_PrintAllNodes();
	printf("+ add t = %lld, value = %lld\n", t, number);
	// позже
	// SBT_Maintain(_root_index, (number >= _nodes[t].number) ? 1 : 0);
	//SBT_RightRotate(_root_index);
	SBT_LeftRotate(_root_index);
			}
			else {
				SBT_Add_At(number, _nodes[t].right, t);
			}
		}
	}
	return 0;
}

int SBT_Add(TNumber number) {
	return SBT_Add_At(number, _root_index, -1);
}

int SBT_Delete(TNumber n) {
	return 0;
}

void SBT_PrintAllNodes_At(int depth, TNodeIndex t) {
	if ((_n_nodes <= 0) || (t < 0)) return; // выйти, если вершины нет
	if (_nodes[t].left >= 0) SBT_PrintAllNodes_At(depth + 1, _nodes[t].left);
	for (int i = 0; i < depth; i++) printf(" "); // отступ
	printf("depth = %d, node = "SBT_FORMAT_STRING": ("SBT_FORMAT_STRING")\n", depth, t, (SBT_FORMAT_TYPE)_nodes[t].number); // иначе: напечатать "тело" узла
	if (_nodes[t].right >= 0) SBT_PrintAllNodes_At(depth + 1, _nodes[t].right);
}

void SBT_PrintAllNodes() {
	printf("_n_nodes = "SBT_FORMAT_STRING"\n", _n_nodes);
	SBT_PrintAllNodes_At(0, _root_index);
	printf("---\n");
}
