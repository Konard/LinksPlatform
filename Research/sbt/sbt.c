
#include "sbt.h"

#include <stdio.h> // printf

#include <pthread.h> // pthread_mutex_*
// узкое место при доступе из многих потоков - глобальная блокировка таблицы _nodes
pthread_mutex_t _lock_nodes = PTHREAD_MUTEX_INITIALIZER;

TNode _nodes[SBT_MAX_NODES];
TNodeIndex _n_nodes = 0; // число вершин в дереве
TNodeIndex _tree_root = -1; // дерево
TNodeIndex _tree_unused = -1; // список неиспользованных
TNodeIndex _n_clean = SBT_MAX_NODES; // хвост "чистых"; только уменьшается - использованные вершины перемещаются в список unused


// Event-driven technique

FuncOnRotate funcOnRotate = NULL;
FuncOnWalk funcOnWalk = NULL;
FuncOnFind funcOnFind = NULL;

int SBT_SetCallback_OnRotate(FuncOnRotate func_) {
	funcOnRotate = func_;
}

int SBT_SetCallback_OnWalk(FuncOnWalk func_) {
	funcOnWalk = func_;
}

int SBT_SetCallback_OnFind(FuncOnFind func_) {
	funcOnFind = func_;
}

// Rotate, Maintain & Add, Delete

// t - слева, перевешиваем туда
// вершины не пропадают, _n_nodes сохраняет значение
int SBT_LeftRotate(TNodeIndex t) {
//	printf("LEFT_ROTATE %lld\n", t);
	if (t < 0) return 0;
	TNodeIndex k = _nodes[t].right;
	if (k < 0) return 0;
	if (funcOnRotate != NULL) funcOnRotate(k, t, "LEFT_ROTATE");

	TNodeIndex p = _nodes[t].parent;

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

	// меняем трёх предков
	// 1. t.right.parent = t
	// 2. k.parent = t.parent
	// 3. t.parent = k
	if (_nodes[t].right != -1) _nodes[_nodes[t].right].parent = t;
	_nodes[k].parent = _nodes[t].parent;
	_nodes[t].parent = k;

	// меняем корень, parent -> t, k
	if (p == -1) { // это root
		_tree_root = k;
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
// вершины не пропадают, _n_nodes сохраняет значение
int SBT_RightRotate(TNodeIndex t) {
//	printf("RIGHT_ROTATE %lld\n", t);
	if (t < 0) return 0;
	TNodeIndex k = _nodes[t].left;
	if (k < 0) return 0;
//	printf("k = %lld\n", k);
	if (funcOnRotate != NULL) funcOnRotate(k, t, "RIGHT_ROTATE");

	TNodeIndex p = _nodes[t].parent;

	// поворачиваем ребро дерева
	_nodes[t].left = _nodes[k].right;
	_nodes[k].right = t;

	// корректируем size
	_nodes[k].size = _nodes[t].size;
	TNodeIndex n_l = _nodes[t].left;
	TNodeIndex n_r = _nodes[t].right;
	TNodeSize s_l = ((n_l != -1) ? _nodes[n_l].size : 0);
	TNodeSize s_r = ((n_r != -1) ? _nodes[n_r].size : 0);
	_nodes[t].size = s_l + s_r + 1;

	// меняем трёх предков
	// 1. t.left.parent = t
	// 2. k.parent = t.parent
	// 3. t.parent = k
	if (_nodes[t].left != -1) _nodes[_nodes[t].left].parent = t;
	_nodes[k].parent = _nodes[t].parent;
	_nodes[t].parent = k;

	// меняем корень, parent -> t, k
	if (p == -1) { // это root
		_tree_root = k;
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

TNodeSize SBT_Left_Left_size(TNodeIndex t) {
	if (t == -1) return 0;
	TNodeIndex l = _nodes[t].left;
	if (l == -1) return 0;
	TNodeIndex ll = _nodes[l].left;
	return ((ll == -1) ? 0 : _nodes[ll].size);
}

TNodeSize SBT_Left_Right_size(TNodeIndex t) {
	if (t == -1) return 0;
	TNodeIndex l = _nodes[t].left;
	if (l == -1) return 0;
	TNodeIndex lr = _nodes[l].right;
	return ((lr == -1) ? 0 : _nodes[lr].size);
}

TNodeSize SBT_Right_Right_size(TNodeIndex t) {
	if (t == -1) return 0;
	TNodeIndex r = _nodes[t].right;
	if (r == -1) return 0;
	TNodeIndex rr = _nodes[r].right;
	return ((rr == -1) ? 0 : _nodes[rr].size);
}

TNodeSize SBT_Right_Left_size(TNodeIndex t) {
	if (t == -1) return 0;
	TNodeIndex r = _nodes[t].right;
	if (r == -1) return 0;
	TNodeIndex rl = _nodes[r].left;
	return ((rl == -1) ? 0 : _nodes[rl].size);
}

TNodeSize SBT_Right_size(TNodeIndex t) {
	if (t == -1) return 0;
	TNodeIndex r = _nodes[t].right;
	return ((r == -1) ? 0 : _nodes[r].size);
}

TNodeSize SBT_Left_size(TNodeIndex t) {
	if (t == -1) return 0;
	TNodeIndex l = _nodes[t].left;
	return ((l == -1) ? 0 : _nodes[l].size);
}

int SBT_Maintain_Simpler(TNodeIndex t, int flag) {

	if (t < 0) return 0;
	TNodeIndex parent = _nodes[t].parent; // есть "родитель"
	int at_left = 0;
	if (parent == -1) { // t - корень дерева, он изменяется; запоминать нужно не индекс, а "топологию"
	}
	else {
	    if (_nodes[parent].left == t) at_left = 1; // "слева" от родителя - индекс родителя не изменился
	    else at_left = 0; // "справа" от родителя
	}

	// поместили слева, flag == 0
	if (flag == 0) {
		if (SBT_Left_Left_size(t) > SBT_Right_size(t)) {
			SBT_RightRotate(t);
		}
		else if (SBT_Left_Right_size(t) > SBT_Right_size(t)) {
			SBT_LeftRotate(_nodes[t].left);
			SBT_RightRotate(t);
		}
		else { return 0; }
	}
	// поместили справа, flag == 1
	else {
		if (SBT_Right_Right_size(t) > SBT_Left_size(t)) {
			SBT_LeftRotate(t);
		}
		else if (SBT_Right_Left_size(t) > SBT_Left_size(t)) {
			SBT_RightRotate(_nodes[t].right);
			SBT_LeftRotate(t);
		}
		else { return 0; }
	}

	TNodeIndex t0 = -1;
	if (parent == -1) t0 = _tree_root;
	else {
	    if (at_left) t0 = _nodes[parent].left;
	    else t0 = _nodes[parent].right;
	}
	SBT_Maintain_Simpler(_nodes[t0].left, 0); // false
	SBT_Maintain_Simpler(_nodes[t0].right, 1); // true
	SBT_Maintain_Simpler(t0, 0); // false
	SBT_Maintain_Simpler(t0, 1); // true

	return 0;
}

int SBT_Maintain(TNodeIndex t) {

	if (t < 0) return 0;

	TNodeIndex parent = _nodes[t].parent; // есть "родитель"
	int at_left = 0;
	if (parent == -1) { // t - корень дерева, он изменяется; запоминать нужно не индекс, а "топологию"
	}
	else {
	    if (_nodes[parent].left == t) at_left = 1; // "слева" от родителя - индекс родителя не изменился
	    else at_left = 0; // "справа" от родителя
	}

#define CALC_T0 \
	TNodeIndex t0 = -1; \
	if (parent == -1) t0 = _tree_root; \
	else { \
	    if (at_left) t0 = _nodes[parent].left; \
	    else t0 = _nodes[parent].right; \
	}

	// поместили слева (?)
	if (SBT_Left_Left_size(t) > SBT_Right_size(t)) {
		SBT_RightRotate(t);
		CALC_T0
		SBT_Maintain(_nodes[t0].right);
		SBT_Maintain(t0);
	}
	else if (SBT_Left_Right_size(t) > SBT_Right_size(t)) {
		SBT_LeftRotate(_nodes[t].left);
		SBT_RightRotate(t);
		CALC_T0
		SBT_Maintain(_nodes[t0].left);
		SBT_Maintain(_nodes[t0].right);
		SBT_Maintain(t0);
	}
	// поместили справа (?)
	else if (SBT_Right_Right_size(t) > SBT_Left_size(t)) {
		SBT_LeftRotate(t);
		CALC_T0
		SBT_Maintain(_nodes[t0].left);
		SBT_Maintain(t0);
	}
	else if (SBT_Right_Left_size(t) > SBT_Left_size(t)) {
		SBT_RightRotate(_nodes[t].right);
		SBT_LeftRotate(t);
		CALC_T0
		SBT_Maintain(_nodes[t0].left);
		SBT_Maintain(_nodes[t0].right);
		SBT_Maintain(t0);
	}

	return 0;
}

// родительская у t - parent
int SBT_AddNode_At(TNumber value, TNodeIndex t, TNodeIndex parent) {
	_nodes[t].size++;
	if (_n_nodes <= 0) {
		TNodeIndex t_new = SBT_AllocateNode();
		_nodes[t_new].value = value;
		_nodes[t_new].parent = parent;
		_nodes[t_new].left = -1;
		_nodes[t_new].right = -1;
		_nodes[t_new].size = 1;
		_tree_root = 0;
	}
	else {
		if(value < _nodes[t].value) {
			if(_nodes[t].left == -1) {
				TNodeIndex t_new = SBT_AllocateNode();
				_nodes[t].left = _n_nodes;
				_nodes[t_new].value = value;
				_nodes[t_new].value = value;
				_nodes[t_new].parent = t;
				_nodes[t_new].left = -1;
				_nodes[t_new].right = -1;
				_nodes[t_new].size = 1;
			}
			else {
				SBT_AddNode_At(value, _nodes[t].left, t);
			}
		}
		else {
			if(_nodes[t].right == -1) {
				TNodeIndex t_new = SBT_AllocateNode();
				_nodes[t].right = _n_nodes;
				_nodes[t_new].value = value;
				_nodes[t_new].value = value;
				_nodes[t_new].parent = t;
				_nodes[t_new].left = -1;
				_nodes[t_new].right = -1;
				_nodes[t_new].size = 1;
			}
			else {
				SBT_AddNode_At(value, _nodes[t].right, t);
			}
		}
	}
	//SBT_Maintain(t);
	SBT_Maintain_Simpler(t, (value >= _nodes[t].value) ? 1 : 0);
	return 0;
}

int SBT_AddNode(TNumber value) {
	return SBT_AddNode_At(value, _tree_root, -1);
}

// Uniq

int SBT_AddNodeUniq(TNumber value) {
	int result = SBT_FindFirstNode(value); // fail, если вершина с таким value уже существует
//	printf("%d\n", result);
	if (result == -1) {
		SBT_AddNode(value);
	}
	return result;
}

int SBT_DeleteNode_At(TNumber value, TNodeIndex t, TNodeIndex parent) {

	int result = -1;
	if (t < 0) return -1; // ответ: "Не найден"

	if (value == _nodes[t].value) {
//		printf("delete %lld\n", t);
//		SBT_PrintAllNodes();

		// Вершину нашли,
		// среагировать на найденный элемент
		if (parent == -1) { // если это - корень дерева
		    if (_nodes[t].left != -1) {
			_tree_root = _nodes[t].left;

			// TNodeIndex 
			_nodes[_tree_root].parent = -1;
			_nodes[_tree_root].right = _nodes[t].right;
			if (_nodes[t].right != -1) {
				_nodes[_nodes[t].right].parent = _tree_root;
			}
		    }
		    else if (_nodes[t].right != -1) {
			_tree_root = _nodes[t].right;

			// TNodeIndex 
			_nodes[_tree_root].parent = -1;
			_nodes[_tree_root].left = _nodes[t].left;
			if (_nodes[t].left != -1) {
				_nodes[_nodes[t].left].parent = _tree_root;
			}
		    }
		    else {
//			printf("delete root\n");
			_tree_root = -1;
		    }
		}
		else {
//			printf("parent != -1\n");

			int at_left = 0;
			if (_nodes[parent].left == t) at_left = 1;
			else at_left = 0;


		    if (_nodes[t].left != -1) {

			// ссылка от parent != -1
			if (at_left == 1) _nodes[parent].left = _nodes[t].left;
			else _nodes[parent].right = _nodes[t].left;

			_nodes[_nodes[t].left].parent = parent; // новый корень, .left
			_nodes[_nodes[t].left].right = _nodes[t].right; // перевешиваем вершину
			if (_nodes[t].right != -1) {
				_nodes[_nodes[t].right].parent = _nodes[t].left;
			}
		    }
		    else if (_nodes[t].right != -1) {

			// ссылка от parent != -1
			if (at_left == 1) _nodes[parent].left = _nodes[t].right;
			else _nodes[parent].right = _nodes[t].right;

			_nodes[_nodes[t].right].parent = parent; // новый корень, .right
			_nodes[_nodes[t].right].left = _nodes[t].left; // перевешиваем вершину
			if (_nodes[t].left != -1) {
				_nodes[_nodes[t].left].parent = _nodes[t].right;
			}
		    }
		    else {
//			printf("> удаление единицы, parent = %lld\n", parent);
			// удалить соответствующее направление у parent
//			SBT_DumpAllNodes();
			if (at_left == 1) _nodes[parent].left = -1;
			else _nodes[parent].right = -1;
//			SBT_PrintAllNodes();
		    }

		}
		_nodes[t].parent = -1;
		_nodes[t].left = -1;
		_nodes[t].right = -1;
		result = t;
	}
	else if (value < _nodes[t].value) {
		// влево
		result = SBT_DeleteNode_At(value, _nodes[t].left, t);
	}
	// можно не делать это сравнение для целых чисел
	else 
	// if (value > _nodes[t].value)
	{
		// вправо
		result = SBT_DeleteNode_At(value, _nodes[t].right, t);
	}
	if (parent != -1) SBT_Maintain(parent);
//	SBT_Maintain(t, (value < _nodes[t].value) ? 1: 0);

	// не выполняется
	if (result != -1) SBT_FreeNode(result);
	return result; // "не найден"
}

int SBT_DeleteNode(TNumber value) {
	TNodeIndex t = SBT_DeleteNode_At(value, _tree_root, -1);
	return t;
}

int SBT_DeleteAll(TNumber value) {
	int result = -1;
	while ((result = SBT_DeleteNode_At(value, _tree_root, -1)) != -1);
	return result;
}

void SBT_PrintAllNodes_At(int depth, TNodeIndex t) {
	if ((_n_nodes <= 0) || (t < 0)) return; // выйти, если вершины нет

	// сверху - большие вершины
	if (_nodes[t].right >= 0) SBT_PrintAllNodes_At(depth + 1, _nodes[t].right);

	if (!((_nodes[t].parent == -1) && (_nodes[t].left == -1) && (_nodes[t].right == -1)) || (t == _tree_root)) {
		for (int i = 0; i < depth; i++) printf(" "); // отступ
		printf("depth = %d, node = %lld: (%lld), size = %lld\n",
			depth,
			(long long int)t,
			(long long int)_nodes[t].value,
			(long long int)_nodes[t].size
		); // иначе: напечатать "тело" узла
	}

	// снизу - меньшие
	if (_nodes[t].left >= 0) SBT_PrintAllNodes_At(depth + 1, _nodes[t].left);
}

void SBT_PrintAllNodes() {
	printf("---\n");
	printf("_n_nodes = %lld\n",
		(long long int)_n_nodes
	);
	SBT_PrintAllNodes_At(0, _tree_root);
	printf("---\n");
}

void SBT_WalkAllNodes_At(int depth, TNodeIndex t) {
	if ((_n_nodes <= 0) || (t < 0)) {
		funcOnWalk(t, -1, "WALK_UP");
		return; // выйти, если вершины нет
	}

	// снизу - меньшие
	if (_nodes[t].left >= 0) {
		funcOnWalk(t, _nodes[t].left, "WALK_DOWN_LEFT");
		SBT_WalkAllNodes_At(depth + 1, _nodes[t].left);
	}
	funcOnWalk(t, t, "WALK_NODE");
	// сверху - большие вершины
	if (_nodes[t].right >= 0) {
		funcOnWalk(t, _nodes[t].right, "WALK_DOWN_RIGHT");
		SBT_WalkAllNodes_At(depth + 1, _nodes[t].right);
	}
	funcOnWalk(t, -1, "WALK_UP");

}

void SBT_WalkAllNodes() {
	funcOnWalk(-1, -1, "WALK_STARTS");
	SBT_WalkAllNodes_At(0, _tree_root);
	funcOnWalk(-1, -1, "WALK_FINISH");
}



TNodeIndex SBT_FindFirstNode_At(TNumber value, TNodeIndex t) {

	if (t < 0) return -1; // ответ: "Не найден"

	if (value == _nodes[t].value) {
		// Среагировать на найденный элемент
		return t;
	}
	else if (value < _nodes[t].value) {
		// влево
		return SBT_FindFirstNode_At(value, _nodes[t].left);
	}
	// можно не делать это сравнение для целых чисел
	else 
	// if (value > _nodes[t].value)
	{
		// вправо
		return SBT_FindFirstNode_At(value, _nodes[t].right);
	}

	// не выполняется
	return -1; // "не найден"
}

TNodeIndex SBT_FindFirstNode(TNumber value) {
	if (_n_nodes <= 0) return -1;
//	printf("root = %lld\n", _tree_root);
	return SBT_FindFirstNode_At(value, _tree_root);
}



void SBT_FindAllNodes_At(TNumber value, TNodeIndex t) {
}

void SBT_FindAllNodes(TNumber value) {
	return SBT_FindAllNodes_At(value, _tree_root);
}



void SBT_CheckAllNodes_At(int depth, TNodeIndex t) {
	if ((_n_nodes <= 0) || (t < 0)) {
		return; // выйти, если вершины нет
	}

	// снизу - меньшие
	if (_nodes[t].left >= 0) {
		SBT_CheckAllNodes_At(depth + 1, _nodes[t].left);
	}
	// проверить
	if ((SBT_Left_Left_size(t) > SBT_Right_size(t)) && (SBT_Right_size(t) > 0)) {
		printf("ERROR %lld LL > R\n",
			(long long int)t,
			(long long int)SBT_Left_Left_size(t),
			(long long int)SBT_Right_size(t)
		);
	}
	if ((SBT_Left_Right_size(t) > SBT_Right_size(t)) && (SBT_Right_size(t) > 0)) {
		printf("ERROR %lld LR > R\n",
			(long long int)t,
			(long long int)SBT_Left_Right_size(t),
			(long long int)SBT_Right_size(t)
		);
	}
	if ((SBT_Right_Right_size(t) > SBT_Left_size(t)) && (SBT_Left_size(t) > 0)) {
		printf("ERROR %lld RR > L (%lld > %lld)\n",
			(long long int)t,
			(long long int)SBT_Right_Right_size(t),
			(long long int)SBT_Left_size(t)
		);
	}
	if ((SBT_Right_Left_size(t) > SBT_Left_size(t)) && (SBT_Left_size(t) > 0)) {
		printf("ERROR %lld RL > L\n",
			(long long int)t,
			(long long int)SBT_Right_Left_size(t),
			(long long int)SBT_Left_size(t)
		);
	}
	
	// сверху - большие вершины
	if (_nodes[t].right >= 0) {
		SBT_CheckAllNodes_At(depth + 1, _nodes[t].right);
	}

}

void SBT_CheckAllNodes() {
	SBT_CheckAllNodes_At(0, _tree_root);
}

void SBT_DumpAllNodes() {
	for (uint64_t i = 0; i < SBT_MAX_NODES - _n_clean; i++) {
		printf("idx = %lld, numb = %lld, size = %lld, left = %lld, right = %lld, parent = %lld (unused = %d)\n",
			(long long int)i,
			(long long int)_nodes[i].value,
			(long long int)_nodes[i].size,
			(long long int)_nodes[i].left,
			(long long int)_nodes[i].right,
			(long long int)_nodes[i].parent,
			(int)_nodes[i].unused
		);
	}
}

TNode *GetNode(TNodeIndex t) {
	return &(_nodes[t]);
}

TNodeIndex GetRootIndex() {
	return _tree_root;
}

TNodeIndex SBT_AllocateNode() {
	TNodeIndex t = -1;
	if (_tree_unused != -1) {
		// выделить из списка
		TNodeIndex t = _tree_unused;
		_tree_unused = _nodes[t].right;
		_nodes[t].left = _nodes[_tree_unused].left;
		_nodes[_tree_unused].left = -1; // теперь - первый элемент
		// на всякий случай - обнуляем (дополнительная очистка)
		_nodes[t].left = -1;
		_nodes[t].right = -1;
		_nodes[t].parent = -1;
		_nodes[t].size = 0;
		_nodes[t].value = 0;
		_nodes[t].unused = 0;
		// счетчика _n_unused нет (но можно добавить)
		_n_nodes++;
		return t;
	}
	else {
		// выделить из чистого списка
		if (_n_clean > 0) {
			
			t = SBT_MAX_NODES - _n_clean;
			printf("clean: %lld\n", (long long int)t);
			_n_clean--;
			_n_nodes++;
			_nodes[t].left = -1;
			_nodes[t].right = -1;
			_nodes[t].parent = -1;
			_nodes[t].size = 0;
			_nodes[t].value = 0;
			_nodes[t].unused = 0;
		}
		else {
			t = -1;
		}
		return t;
	}
}

// небезопасная функция ! следите за целостностью дерева самостоятельно !
// подключение в "кольцевой буфер"
int SBT_FreeNode(TNodeIndex t) {
	// переместить в unused-пространство
	if (_tree_unused == -1) {
		_tree_unused = t;
		_nodes[t].left = t;
		_nodes[t].right = t;
		_nodes[t].parent = -1;
		_nodes[t].size = 0;
		_nodes[t].value = 0;
		_nodes[t].unused = 1;
		// счетчика _n_unused нет
	}
	else {
		_nodes[t].left = _nodes[_tree_unused].left;
		_nodes[_tree_unused].left = t;
		_nodes[t].right = _tree_unused;
		_nodes[t].parent = -1;
		_nodes[t].size = 0;
		_nodes[t].value = 0;
		_nodes[t].unused = 1;
		// счетчика _n_unused нет
	}
	_n_nodes--;
}
