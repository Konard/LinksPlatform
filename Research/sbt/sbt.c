
#include "sbt.h"

#include <stdio.h> // printf

#ifdef __LINUX__
#include <pthread.h> // pthread_mutex_*
// узкое место при доступе из многих потоков - глобальная блокировка таблицы _nodes
pthread_mutex_t _lock_nodes = PTHREAD_MUTEX_INITIALIZER;
#endif

// переменные модуля

TNode _nodes[SBT_MAX_NODES];
TNodeIndex _n_nodes = 0; // число вершин в дереве
TNodeIndex _tree_root = -1; // дерево
TNodeIndex _tree_unused = -1; // список неиспользованных
TNodeIndex _n_clean = SBT_MAX_NODES; // хвост "чистых"; только уменьшается - использованные вершины перемещаются в список unused


// Event-driven technique, функции для оповещения о событиях

FuncOnRotate funcOnRotate = NULL;
FuncOnWalk funcOnWalk = NULL;
FuncOnFind funcOnFind = NULL;

int SBT_SetCallback_OnRotate(FuncOnRotate func_) {
	funcOnRotate = func_;
	return 0;
}

int SBT_SetCallback_OnWalk(FuncOnWalk func_) {
	funcOnWalk = func_;
	return 0;
}

int SBT_SetCallback_OnFind(FuncOnFind func_) {
	funcOnFind = func_;
	return 0;
}


// Функции Rotate, Maintain & Add, Delete //

// Вращение влево, t - слева, перевешиваем туда (вершины не пропадают, _n_nodes сохраняет значение)

int SBT_LeftRotate(TNodeIndex t) {

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

// Вращение вправо, t - справа, перевешиваем туда (вершины не пропадают, _n_nodes сохраняет значение)

int SBT_RightRotate(TNodeIndex t) {

	if (t < 0) return 0;
	TNodeIndex k = _nodes[t].left;
	if (k < 0) return 0;
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

// Размеры для вершин, size

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

// Сбалансировать дерево (более быстрый алгоритм)

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

// Сбалансировать дерево ("тупой" алгоритм)

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


// Добавить вершину в поддерево t, без проверки уникальности (куда - t, родительская - parent)

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
				_nodes[t].left = t_new;
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
				_nodes[t].right = t_new; // по-умолчанию, добавляем вправо (поэтому "левых" вращений больше)
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

// Добавить вершину без проверки уникальности value

int SBT_AddNode(TNumber value) {
	return SBT_AddNode_At(value, _tree_root, -1);
}

// Добавление вершины только если такой же (с таким же значением) в дереве нет

int SBT_AddNodeUniq(TNumber value) {

	int result = SBT_FindNode(value); // fail, если вершина с таким value уже существует
	if (result == -1) {
		SBT_AddNode(value);
	}
	return result;
}

// Удалить вершину, в дереве t
// Алгоритм (взят из статьи про AVL на Википедии) :
// 1. Ищем вершину на удаление,
// 2. Ищем ей замену, спускаясь по левой ветке (value-),
// 3. Перевешиваем замену на место удаленной,
// 4. Выполняем балансировку вверх от родителя места, где находилась замена (от parent замены).

int SBT_DeleteNode_At(TNumber value, TNodeIndex t, TNodeIndex parent) {
	if ((_n_nodes <= 0) || (t < 0)) {
		return -1; // ответ: "Не найден"
	}

//	int result = -1;
	TNodeIndex d = SBT_FindNode(value);
	printf("idx(d) = %lld\n", (long long int)d);
	// надо ли что-то делать? (вершину нашли?)
	if (d != -1) {
		TNodeIndex d_p = (d != -1) ? _nodes[d].parent : -1;
		printf("idx(d_p) = %lld\n", (long long int)d_p);
		TNodeIndex l = SBT_FindNode_NearestAndLesser_ByIndex(d);
		printf("idx(l) = %lld\n", (long long int)l);
		// l == -1, только если у t_d нет дочерних вершин слева (хотя бы одной, <= t_d),
		// в таком случае - просто удаляем t_d (без перевешивания)
		
		if (l == -1) {
			TNodeIndex r = SBT_FindNode_NearestAndGreater_ByIndex(d);
			printf("idx(r) = %lld\n", (long long int)r);
			// (Diagram No.3)
			if (r == -1) {
				// вершина d является листом
				// меняем ссылку на корень
				if (d_p == -1) _tree_root = -1;
				else {
					if (_nodes[d_p].left == d) _nodes[d_p].left = -1;
					else _nodes[d_p].right = -1;
				}
				_nodes[d].parent = -1;
				// больше ничего делать не надо
			}
			// r != -1 (Diagram No.2)
			else {
				// меняем справа

				TNodeIndex r_p = _nodes[r].parent;
				TNodeIndex r_r = _nodes[r].right; // r_l = r.left == -1
				TNodeIndex d_l = _nodes[d].left;
				printf("idx(r_p) = %lld\n", (long long int)r_p);
				printf("idx(r_r) = %lld\n", (long long int)r_r);
				printf("idx(d_l) = %lld\n", (long long int)d_l);
				
				// меняем правую часть l
				_nodes[r].left = d_l;
				if (d_l != -1) _nodes[d_l].parent = r;
				
				// меняем левую часть l
				if (r_p != d) {
					_nodes[r].right = r_p;
						_nodes[r_p].left = r_r;
				}
				else {
					// не надо менять левую часть
				}
				// меняем ссылку на корень
				if (d_p == -1){
					// меняем l <-> root = t_d_p = -1 (1)
					_tree_root = r;
					_nodes[r].parent = -1;
				}
				else {
					// меняем l <-> root = t_d_p != -1
					_nodes[d_p].left = r;
					_nodes[r].parent = d_p;
				}
			}
		}
		// l != -1 (Diagram No.1)
		else {
			TNodeIndex l_p = _nodes[l].parent;
			TNodeIndex l_l = _nodes[l].left; // l_r = l.right == -1
			TNodeIndex d_r = _nodes[d].right;
			printf("idx(l_p) = %lld\n", (long long int)l_p);
			printf("idx(l_l) = %lld\n", (long long int)l_l);
			printf("idx(d_r) = %lld\n", (long long int)d_r);
			
			// меняем правую часть l
			_nodes[l].right = d_r;
			if (d_r != -1) _nodes[d_r].parent = l;
			
			// меняем левую часть l
			if (l_p != d) {
				// l_p <-> d.left
				TNodeIndex d_l = _nodes[d].left;
				if (d_l != -1) {
					_nodes[l].left = d_l;
					_nodes[d_l].parent = l;
				}
				// l_p <-> l_l
				_nodes[l_p].right = l_l;
				if (l_l != -1) _nodes[l_l].parent = l_p;
			}
			else {
				// не надо менять левую часть
			}
			printf("*\n");

			// меняем ссылку на корень
			if (d_p == -1){
				// меняем l <-> root = t_d_p = -1 (1)
				_tree_root = l;
				_nodes[l].parent = -1;
			}
			else {
				// меняем l <-> root = t_d_p != -1
				_nodes[d_p].right = l;
				_nodes[l].parent = d_p;
			}

		}
		SBT_FreeNode(d);
	}

	return t;
}

int SBT_DeleteNode_At_Old(TNumber value, TNodeIndex t, TNodeIndex parent) {

	if ((_n_nodes <= 0) || (t < 0)) {
		return -1; // ответ: "Не найден"
	}

	int result = -1;

	// Вершину нашли, среагировать на найденный элемент t
	if (value == _nodes[t].value) {
		TNodeIndex left = _nodes[t].left;
		TNodeIndex right = _nodes[t].right;
		// если это - корень дерева
		if (parent == -1) {
			// можно сделать проверку на left.size <?> right.size => удалять с перевесом в соответствующую сторону
			if (left != -1) {
				_tree_root = left;
				_nodes[left].parent = -1;
				_nodes[left].right = right;
				if (right != -1) _nodes[right].parent = left;
			}
			else if (right != -1) {
				_tree_root = right;
				_nodes[right].parent = -1;
				_nodes[right].left = left;
				if (left != -1) _nodes[left].parent = right;
			}
			else _tree_root = -1;
		}
		// не _корень_ дерева
		else {
			int at_left = 0;
			if (_nodes[parent].left == t) at_left = 1; // else делать не нужно, = 0 by-default
			// ссылка от parent налево != -1
			if (left != -1) {
				if (at_left == 1) _nodes[parent].left = left;
				else _nodes[parent].right = left;
				_nodes[left].parent = parent; // новый корень, .left
				_nodes[left].right = right; // перевешиваем вершину
				if (right != -1) _nodes[right].parent = left;
			}
			// ссылка от parent направо != -1
			else if (right != -1) {
				if (at_left == 1) _nodes[parent].left = right;
				else _nodes[parent].right = right;
				_nodes[right].parent = parent; // новый корень, .right
				_nodes[right].left = left; // перевешиваем вершину
				if (left != -1) _nodes[left].parent = right;
			}
			else {
				// нечего подвешивать - удалить соответствующие направления у parent
				if (at_left == 1) _nodes[parent].left = -1;
				else _nodes[parent].right = -1;
			}
		}
		// parent/left/right = -1: делает процедура освобождения ячейки, FreeNode?
		result = t;
		SBT_FreeNode(result);
	}
	else if (value < _nodes[t].value) {
		// влево
		result = SBT_DeleteNode_At(value, _nodes[t].left, t);
	}
	else 
	// это не обязательно if (value > _nodes[t].value) # (можно не делать это сравнение для целых чисел)
	{
		// вправо
		result = SBT_DeleteNode_At(value, _nodes[t].right, t);
	}
//	if (parent != -1) SBT_Maintain(parent);
	if (parent != -1) SBT_Maintain_Simpler(parent, (value < _nodes[t].value) ? 1: 0);
	return result; // "не найден?"
}

// Удалить первую попавшуюся вершину по значению value

int SBT_DeleteNode(TNumber value) {
	TNodeIndex t = SBT_DeleteNode_At(value, _tree_root, -1);
	return t;
}

// Удалить все вершины с данным значением (value)

int SBT_DeleteAllNodes(TNumber value) {
	int result = -1;
	while ((result = SBT_DeleteNode_At(value, _tree_root, -1)) != -1);
	return result;
}

// Напечатать вершины в поддереве t

void SBT_PrintAllNodes_At(int depth, TNodeIndex t) {

	if ((_n_nodes <= 0) || (t < 0)) {
		return; // выйти, если вершины нет
	}

	// сверху - большие вершины
	if (_nodes[t].right >= 0) SBT_PrintAllNodes_At(depth + 1, _nodes[t].right);

	if (!((_nodes[t].parent == -1) && (_nodes[t].left == -1) && (_nodes[t].right == -1)) || (t == _tree_root)) {
		for (int i = 0; i < depth; i++) printf(" "); // отступ
		printf("+%d, id = %lld, value = %lld, size = %lld\n",
			depth,
			(long long int)t,
			(long long int)_nodes[t].value,
			(long long int)_nodes[t].size
		); // иначе: напечатать "тело" узла
	}

	// снизу - меньшие
	if (_nodes[t].left >= 0) SBT_PrintAllNodes_At(depth + 1, _nodes[t].left);
}

// Напечатать все вершины, начиная от корня дерева

void SBT_PrintAllNodes() {
	printf("n_nodes = %lld\n", (long long int)_n_nodes);
	SBT_PrintAllNodes_At(0, _tree_root);
}

// Пройтись с вершины t (по поддереву)

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

// Пройтись по всем вершинам, сгенерировать события WALK_ (посылаются в зарегистрированные перед этим обработчики событий)

void SBT_WalkAllNodes() {
	funcOnWalk(-1, -1, "WALK_STARTS");
	SBT_WalkAllNodes_At(0, _tree_root);
	funcOnWalk(-1, -1, "WALK_FINISH");
}

// Найти значение value в поддереве t (корректно при использовании AddNodeUniq)

TNodeIndex SBT_FindNode_At(TNumber value, TNodeIndex t) {

	if ((_n_nodes <= 0) || (t < 0)) {
		return -1; // ответ: "Не найден"
	}

	if (value == _nodes[t].value) {
		// Среагировать на найденный элемент, вернуть индекс этой ноды
		return t;
	}
	else if (value < _nodes[t].value) {
		// влево
		return SBT_FindNode_At(value, _nodes[t].left);
	}
	// можно не делать это сравнение для целых чисел
	else 
	// if (value > _nodes[t].value)
	{
		// вправо
		return SBT_FindNode_At(value, _nodes[t].right);
	}

	// не выполняется
	return -1; // "не найден"
}

// Найти вершину от корня

TNodeIndex SBT_FindNode(TNumber value) {
	if (_n_nodes <= 0) return -1;
	return SBT_FindNode_At(value, _tree_root);
}

// (не реализовано)

void SBT_FindAllNodes_At(TNumber value, TNodeIndex t) {
	return;
}

// (не реализовано)

void SBT_FindAllNodes(TNumber value) {
	return SBT_FindAllNodes_At(value, _tree_root);
}


// Проверка дерева на SBT-корректность, начиная с ноды t

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
		printf("ERROR %lld LL > R (%lld > %lld)\n",
			(long long int)t,
			(long long int)SBT_Left_Left_size(t),
			(long long int)SBT_Right_size(t)
		);
	}
	if ((SBT_Left_Right_size(t) > SBT_Right_size(t)) && (SBT_Right_size(t) > 0)) {
		printf("ERROR %lld LR > R (%lld > %lld)\n",
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
		printf("ERROR %lld RL > L (%lld > %lld)\n",
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

// Проверить всё дерево на SBT-корректность (начиная с корневой ноды)

void SBT_CheckAllNodes() {
	SBT_CheckAllNodes_At(0, _tree_root);
}

// распечатка WORK- и UNUSED- nodes (всё, что до CLEAN-)

void SBT_DumpAllNodes() {
	for (uint64_t i = 0; i < SBT_MAX_NODES - _n_clean; i++) {
		printf("idx = %lld, value = %lld [unused = %d], left = %lld, right = %lld, parent = %lld, size = %lld\n",
			(long long int)i,
			(long long int)_nodes[i].value,
			(int)_nodes[i].unused,
			(long long int)_nodes[i].left,
			(long long int)_nodes[i].right,
			(long long int)_nodes[i].parent,
			(long long int)_nodes[i].size
		);
	}
}

TNode *GetPointerToNode(TNodeIndex t) {
	return &(_nodes[t]);
}

TNodeIndex GetRootIndex() {
	return _tree_root;
}


// memory management, эти функции ничего не должны знать о _структуру_ дерева
// значения .left/.right и т.п. они используют лишь для выстраивания элементов в список

// выделение памяти из кольцевого FIFO-буфера (UNUSED-нодов), иначе - из массива CLEAN-нодов

TNodeIndex SBT_AllocateNode() {

	TNodeIndex t = -1; // нет ноды

	// выделить из UNUSED-области (списка)
	if (_tree_unused != -1) {
		// вершины в списке есть
		TNodeIndex t = _tree_unused; // берем из начала
		_tree_unused = _nodes[t].right; // перемещаем указатель на следующий элемент списка
		TNodeIndex last = _nodes[t].left;
		// теперь first.right - первый элемент, на него ссылается last
		_nodes[_tree_unused].left = t;
		_nodes[last].right = _tree_unused;
	}

	// выделить из CLEAN-области (чистые ячейки)
	else {
		if (_n_clean > 0) {
			
			t = SBT_MAX_NODES - _n_clean;
			_n_clean--;
		}
		else {
			t = -1; // память закончилась
			return t;
		}
	}

	// дополнительная очистка
	_nodes[t].left = -1;
	_nodes[t].right = -1;
	_nodes[t].parent = -1;
	_nodes[t].size = 0;
	_nodes[t].value = 0;
	_nodes[t].unused = 0;
	// счетчика _n_unused нет (но можно добавить)
	_n_nodes++;

	return t; // результат (нода)
}

// небезопасная функция ! следите за целостностью дерева самостоятельно !
// подключение в "кольцевой буфер"
// в плане методики, FreeNode не знает ничего о структуре дерева, она работает с ячейками, управляя только ссылками .left и .right

int SBT_FreeNode(TNodeIndex t) {

	// UNUSED пуст, сделать первым элементом в unused-пространстве
	if (_tree_unused == -1) {
		_nodes[t].left = t;
		_nodes[t].right = t;
		_tree_unused = t;
	}

	// UNUSED уже частично или полностью заполнен, добавить в unused-пространство
	else {
		TNodeIndex last = _nodes[_tree_unused].left; // так как существует root-вершина (first), то и last <> -1
		// ссылки внутри
		_nodes[t].left = last;
		_nodes[last].right = t;
		// ссылки снаружи
		_nodes[_tree_unused].left = t;
		_nodes[t].right = _tree_unused;
		// _tree_unused не изменяется
	}
	_nodes[t].parent = -1;
	_nodes[t].size = 0;
	_nodes[t].value = 0;
	_nodes[t].unused = 1;
	// счетчика _n_unused нет
	_n_nodes--; // условный счетчик (вспомогательный): эти вершины можно пересчитать в WORK-пространстве

	return 0;
}

// Найти самый близкий по значению элемент в "левом" поддереве, by Index

TNodeIndex SBT_FindNode_NearestAndLesser_ByIndex(TNodeIndex t) {
	if (t != -1) {
		TNodeIndex left = _nodes[t].left;
		if (left == -1) {
			t = -1;
		}
		else {
			TNodeIndex parent = t;
			TNodeIndex right = left;
			while (right != -1) {
				parent = right;
				right = _nodes[right].right;
			}
			t = parent; // if parent != t[value]
		}
	}
	// else not found, = -1
	return t;
}

// Найти самый близкий по значению элемент в "правом" поддереве, by Index

TNodeIndex SBT_FindNode_NearestAndGreater_ByIndex(TNodeIndex t) {
	if (t != -1) {
		TNodeIndex right = _nodes[t].right;
		if (right == -1) {
			t = -1;
		}
		else {
			TNodeIndex parent = t;
			TNodeIndex left = right;
			while (left != -1) {
				parent = left;
				left = _nodes[left].left;
			}
			t = parent; // if parent != t[value]
		}
	}
	return t;
}

// Найти самый близкий по значению элемент в "левом" поддереве

TNodeIndex SBT_FindNode_NearestAndLesser_ByValue(TNumber value) {
	return SBT_FindNode_NearestAndLesser_ByIndex(SBT_FindNode(value));
}

// Найти самый близкий по значению элемент в "правом" поддереве

TNodeIndex SBT_FindNode_NearestAndGreater_ByValue(TNumber value) {
	return SBT_FindNode_NearestAndGreater_ByIndex(SBT_FindNode(value));
}

// INT64_MAX, если не можем найти элемент по индексу

TNumber GetValueByIndex(TNodeIndex t) {
	return (t != 1) ? _nodes[t].value : INT64_MAX;
}
