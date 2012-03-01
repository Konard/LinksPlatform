#ifndef __SBT_H__
#define __SBT_H__

#define SBT_MAX_NODES 1000000
// C language does not mean constant, const int SBT_MAX_NODES = 1000000; // 1 млн. узлов

#include <stdint.h>
typedef uint64_t TNumber;
typedef uint64_t TNodeIndex;

typedef struct TNode {
    TNumber number;
    TNodeIndex left; // служебные поля
    TNodeIndex right;
} TNode;

int SBT_Add(TNumber n);
void SBT_PrintAllNodes();

#endif
