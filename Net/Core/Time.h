#ifndef __LINK_TIME_H__
#define __LINK_TIME_H__

#ifdef _MFC_VER
long long GetTimestamp();
#endif

#ifdef __LINUX__
#include <time.h>
uint64_t GetTimestamp();
#endif

#endif
