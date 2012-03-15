#ifndef __SBT_H__
#define __SBT_H__

#define SBT_MAX_NODES 1000000
// C language does not mean constant, const int SBT_MAX_NODES = 1000000; // 1 млн. узлов
// см. http://compgroups.net/comp.lang.c/Explanation-needed-for-const-int-error-variably-modified-at-file-scope

#include <stdint.h>

typedef int64_t TNumber;
typedef int64_t TNodeIndex;
typedef uint64_t TNodeSize;

typedef struct TNode {
	TNumber value;
	TNodeIndex parent; // уровень выше
	TNodeIndex left;  // служебные поля,
	TNodeIndex right; //  = -1, если нет дочерних вершин
	TNodeSize size; // size в понимании SBT
	// Основной момент в самобалансирующихся деревьях - это критерий "вращений" (rotates).
	// В случае Size Balanced Trees (SBT), балансировка производится на основе вычислений
	// "размеров" (size) некоторых вершин, близких к "корню" балансировки (t)
	int unused; // можно использовать и для других флагов
} TNode;

//typedef enum TTreeAction {
//	SBT_ACTION_LEFT_ROTATE,
//	SBT_ACTION_RIGHT_ROTATE
//} TTreeAction;

// LEFT_ROTATE, RIGHT_ROTATE
typedef int (*FuncOnRotate)(TNodeIndex nodeIndex1, TNodeIndex nodeIndex2, const char *stringAction);

// WALK_DOWN, WALK_UP, WALK_NODE
typedef int (*FuncOnWalk)(TNodeIndex nodeIndex1, TNodeIndex nodeIndex2, const char *stringAction);

// FOUND
typedef int (*FuncOnFind)(TNodeIndex nodeIndex, const char *stringAction);


int SBT_SetCallback_OnRotate(FuncOnRotate func_);
int SBT_SetCallback_OnWalk(FuncOnWalk func_);
int SBT_SetCallback_OnFind(FuncOnFind func_);

// для внутреннего использования или экспериментов
int SBT_LeftRotate(TNodeIndex t);
int SBT_RightRotate(TNodeIndex t);

int SBT_AddNode(TNumber value); // для неуникального ключа
int SBT_AddNodeUniq(TNumber value);
int SBT_DeleteNode(TNumber value); // -1, если нет такого узла в дереве
int SBT_DeleteAll(TNumber value); // для неуникального ключа

TNodeIndex SBT_AllocateNode();
int SBT_FreeNode(TNodeIndex t); // -1, если не удается удалить ... (в данной реализации - всегда = 0)

// Dump & Check

void SBT_CheckAllNodes_At(int depth, TNodeIndex t);
void SBT_CheckAllNodes();

void SBT_PrintAllNodes_At(int depth, TNodeIndex t);
void SBT_PrintAllNodes();

void SBT_DumpAllNodes();

// Search & Walk

TNodeIndex SBT_FindFirstNode_At(TNumber value, TNodeIndex t);
TNodeIndex SBT_FindFirstNode(TNumber value);

void SBT_FindAllNodes_At(TNumber value, TNodeIndex t);
void SBT_FindAllNodes(TNumber value);

void SBT_WalkAllNodes_At(int depth, TNodeIndex t);
void SBT_WalkAllNodes();

// Get

TNode *GetNode(TNodeIndex t);
TNodeIndex GetRootIndex();

#endif
