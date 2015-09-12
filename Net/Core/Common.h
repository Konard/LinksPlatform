#ifndef __LINKS_COMMON_H__
#define __LINKS_COMMON_H__

#define false 0
#define true 1
#define bool unsigned

#include <stdint.h>

// Size for basic types
// Размер для основных типов
typedef uint64_t unsigned_integer;
typedef int64_t signed_integer;
typedef unsigned_integer link_index; // Short for links' array index, unsigned integer (короткая форма для беззнакового индекса в массиве связей)

#if defined(_MSC_VER) || defined(__MINGW32__) || defined(__MINGW64__) // Для Windows: получения .exe/.obj/.dll (Visual C++/MinGW32):
#ifndef WINDOWS
#define WINDOWS
#endif
#elif defined(__linux__)  // Для Linux: получения ./.o/.so:
#ifndef LINUX
#define LINUX
#endif
#endif

// see http://stackoverflow.com/questions/538134/exporting-functions-from-a-dll-with-dllexport
#if defined(WINDOWS)
#if defined(LINKS_DLL_EXPORT) || defined (CORE_EXPORTS)
#define PREFIX_DLL __declspec(dllexport)
//#define _H __stdcall
#define _H 
#else
#define PREFIX_DLL __declspec(dllimport)
#define _H 
#endif
#elif defined(LINUX)
// Linux,Unix
#define PREFIX_DLL 
#define _H 
#endif

#endif
