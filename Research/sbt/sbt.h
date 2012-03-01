#ifndef __SBT_H__
#define __SBT_H__

#define SBT_MAX_NODES 1000000
// C language does not mean constant, const int SBT_MAX_NODES = 1000000; // 1 млн. узлов
// см. http://compgroups.net/comp.lang.c/Explanation-needed-for-const-int-error-variably-modified-at-file-scope

#include <stdint.h>
typedef uint64_t TNumber;
typedef uint64_t TNodeIndex;
typedef uint64_t TNodeSize;

typedef struct TNode {
	TNumber number;
	TNodeIndex left;  // служебные поля,
	TNodeIndex right; //  = -1, если нет дочерних вершин
	TNodeSize size; // size в понимании SBT
} TNode;

int SBT_Add(TNumber n);
int SBT_Delete(TNumber n);
void SBT_PrintAllNodes_At(TNodeIndex t);
void SBT_PrintAllNodes();

#endif
