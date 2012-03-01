#ifndef __SBT_H__
#define __SBT_H__

#define SBT_MAX_NODES 1000000
// C language does not mean constant, const int SBT_MAX_NODES = 1000000; // 1 млн. узлов
// см. http://compgroups.net/comp.lang.c/Explanation-needed-for-const-int-error-variably-modified-at-file-scope

#include <stdint.h>
#define SBT_FORMAT_STRING "%lld"
#define SBT_FORMAT_TYPE long long signed int
typedef int64_t TNumber;
typedef int64_t TNodeIndex;
typedef uint64_t TNodeSize;

typedef struct TNode {
	TNumber number;
	TNodeIndex parent; // уровень выше
	TNodeIndex left;  // служебные поля,
	TNodeIndex right; //  = -1, если нет дочерних вершин
	TNodeSize size; // size в понимании SBT
} TNode;

int SBT_Add(TNumber number);
int SBT_Delete(TNumber number);
void SBT_PrintAllNodes_At(int depth, TNodeIndex t);
void SBT_PrintAllNodes();

#endif
