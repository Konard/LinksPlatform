#ifndef __LINKS_COMMON_H__
#define __LINKS_COMMON_H__

/*
// для Visual Studio C++
#pragma inline_recursion(on)
#pragma inline_depth(2)
*/

// Проверка на 64-битность (если не так - выдается предупреждение (warning))
// see http://stackoverflow.com/questions/163058/how-can-i-detect-if-im-compiling-for-a-64bits-architecture-in-c
#if defined(_LP64) || defined(__amd64__) || defined(_M_X64)
#else
#warning "Links platform needs 64-bit CPU architecture."
#endif

#define null 0
#define false 0
#define true 1
#define bool unsigned

#define itself 0

// индекс линка unused
#define LINK_0 0

// Определение 32-ти и 64-битных типов
#if defined(_MSC_VER) || defined(__MINGW32__)
typedef __int32 int32_t;
typedef unsigned __int32 uint32_t;
typedef __int64 int64_t;
typedef unsigned __int64 uint64_t;
#else
#include <stdint.h>
#endif


#endif
