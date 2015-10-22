#ifndef __LINKS_COMMON_H__
#define __LINKS_COMMON_H__

#if defined(_MSC_VER) || defined(__MINGW32__) || defined(__MINGW64__) // Для Windows: получения .exe/.obj/.dll (Visual C++/MinGW32):
#ifndef WINDOWS
#define WINDOWS
#endif
#elif defined(__linux__)  // Для Linux: получения ./.o/.so:
#ifndef LINUX
#define LINUX
#endif
#endif

#ifndef EXIT_SUCCESS
#define EXIT_SUCCESS 0
#endif

#ifndef TRUE
#define TRUE 1
#endif

#endif
