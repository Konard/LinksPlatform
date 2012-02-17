#ifndef __LINKS_COMMON_H__
#define __LINKS_COMMON_H__

/*
#pragma inline_recursion(on)
#pragma inline_depth(2)
*/

//#include <omp.h>

#define null 0
#define false 0
#define true 1
#define bool unsigned

#ifdef _MSC_VER
typedef __int32 int32_t;
typedef unsigned __int32 uint32_t;
typedef __int64 int64_t;
typedef unsigned __int64 uint64_t;
#else
#include <stdint.h>
#endif


#endif
