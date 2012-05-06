#ifndef __LINKS_COMMON_H__
#define __LINKS_COMMON_H__

// see http://stackoverflow.com/questions/163058/how-can-i-detect-if-im-compiling-for-a-64bits-architecture-in-c
#if defined(_LP64) || defined(__amd64__) || defined(_M_X64)
#else
// what is for Windows? #warning "Links platform needs 64-bit CPU architecture."
#endif

#define false 0
#define true 1
#define bool unsigned

#if defined(_MSC_VER)
typedef __int32 int32_t;
typedef unsigned __int32 uint32_t;
typedef __int64 int64_t;
typedef unsigned __int64 uint64_t;
#else
#include <stdint.h>
#endif

#endif
