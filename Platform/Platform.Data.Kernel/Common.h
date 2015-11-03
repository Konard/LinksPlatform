#ifndef __LINKS_COMMON_H__
#define __LINKS_COMMON_H__

#define false 0
#define true 1
#define bool unsigned

#include <stdint.h>
#include <inttypes.h>

// Size for basic types
// Размер для основных типов
typedef uint64_t unsigned_integer; // Unsigned integer (Беззнаковое целое число)
typedef int64_t signed_integer; // Signed integer (Целое число со знаком)
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
//#define public_calling_convention __stdcall
#define public_calling_convention
#else
#define PREFIX_DLL __declspec(dllimport)
#define public_calling_convention 
#endif
#elif defined(LINUX)
// Linux,Unix
#define PREFIX_DLL 
#define public_calling_convention 
#define __forceinline /* __inline__ __attribute__((always_inline)) */
#endif

#ifdef _DEBUG
#define DEBUG
#endif

#ifdef DEBUG
#include <stdio.h>
#endif

#define SUCCESS_RESULT 1
#define succeeded(x) SUCCESS_RESULT == (x)
#define ERROR_RESULT 0
#define failed(x) SUCCESS_RESULT != (x)

__forceinline signed_integer Error(char* message);

__forceinline signed_integer ErrorWithCode(char* message, signed_integer errorCode);

__forceinline void DebugInfo(char* message);

#endif
