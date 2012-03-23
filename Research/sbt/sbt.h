#ifndef __SBT_H__
#define __SBT_H__

#define SBT_MAX_NODES 10000000
// C language does not mean constant, const int SBT_MAX_NODES = 1000000; // 1 млн. узлов
// см. http://compgroups.net/comp.lang.c/Explanation-needed-for-const-int-error-variably-modified-at-file-scope

#include <stdint.h>

typedef int64_t TNumber;
typedef int64_t TNodeIndex;
typedef uint64_t TNodeSize;

typedef struct TNode {
	TNumber value; // значение, привязанное к ноде
	TNodeIndex parent; // уровень выше
	TNodeIndex left;  // служебные поля,
	TNodeIndex right; //  = -1, если нет дочерних вершин
	TNodeSize size; // size в понимании SBT
	// Основной момент в самобалансирующихся деревьях - это критерий "вращений" (rotates).
	// В случае Size Balanced Trees (SBT), балансировка производится на основе вычислений
	// "размеров" (size) некоторых вершин, близких к "корню" балансировки (t)
	int unused; // можно использовать и для других флагов
} TNode;

// типы функций - обработчиков событий

typedef int (*FuncOnRotate)(TNodeIndex nodeIndex1, TNodeIndex nodeIndex2, const char *stringAction); // LEFT_ROTATE, RIGHT_ROTATE
typedef int (*FuncOnWalk)(TNodeIndex nodeIndex1, TNodeIndex nodeIndex2, const char *stringAction); // WALK_DOWN, WALK_UP, WALK_NODE
typedef int (*FuncOnFind)(TNodeIndex nodeIndex, const char *stringAction); // FOUND

// для оповещения о событиях

int SBT_SetCallback_OnRotate(FuncOnRotate func_);
int SBT_SetCallback_OnWalk(FuncOnWalk func_);
int SBT_SetCallback_OnFind(FuncOnFind func_);

// для внутреннего использования или экспериментов
int SBT_LeftRotate(TNodeIndex t);
int SBT_RightRotate(TNodeIndex t);

// Основные функции

int SBT_AddNode(TNumber value); // для неуникального ключа
int SBT_AddNodeUniq(TNumber value);
int SBT_DeleteNode(TNumber value); // -1, если нет такого узла в дереве
int SBT_DeleteAllNodes(TNumber value); // для неуникального ключа

TNodeIndex SBT_AllocateNode();
int SBT_FreeNode(TNodeIndex t); // -1, если не удается удалить ... (в данной реализации - всегда = 0)

// Print, Dump & Check

void SBT_CheckAllNodesBalance_At(int depth, TNodeIndex t);
void SBT_CheckAllNodesBalance();

void SBT_CheckAllNodesSize_At(int depth, TNodeIndex t);
void SBT_CheckAllNodesSize();

void SBT_CheckAllNodes(); // balance + size

void SBT_PrintAllNodes_At(int depth, TNodeIndex t);
void SBT_PrintAllNodes();

void SBT_DumpAllNodes();

// Search & Walk

TNodeIndex SBT_FindNode_At(TNumber value, TNodeIndex t);
TNodeIndex SBT_FindNode(TNumber value);

void SBT_FindAllNodes_At(TNumber value, TNodeIndex t);
void SBT_FindAllNodes(TNumber value);

void SBT_WalkAllNodes_At(int depth, TNodeIndex t);
void SBT_WalkAllNodes();

TNodeIndex SBT_FindNode_NearestAndLesser_ByIndex(TNodeIndex t);
TNodeIndex SBT_FindNode_NearestAndGreater_ByIndex(TNodeIndex t);

TNodeIndex SBT_FindNode_NearestAndLesser_ByValue(TNumber value);
TNodeIndex SBT_FindNode_NearestAndGreater_ByValue(TNumber value);

// Get

TNode *GetPointerToNode(TNodeIndex t);
TNodeIndex GetRootIndex();
TNumber GetValueByIndex(TNodeIndex t);

#endif
